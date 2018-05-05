using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Assembly.Metro.Controls.PageTemplates.Games.Components.MetaData
{
	public enum PackedIntType
	{
		PackedInt8,
		PackedInt16,
		PackedInt32
	}

	public class PackedIntData : ValueField
	{
		private readonly SortedList<int, IntData> _ints = new SortedList<int, IntData>();
		private PackedIntType _type;
		private uint _value;

		public PackedIntData(string name, uint offset, uint address, PackedIntType type, uint pluginLine)
			: base(name, offset, address, pluginLine)
		{
			_type = type;
		}

		public uint Value
		{
			get { return _value; }
			set
			{
				_value = value;
				NotifyPropertyChanged("Value");
				RefreshInts();
			}
		}

		public PackedIntType Type
		{
			get { return _type; }
			set
			{
				_type = value;
				NotifyPropertyChanged("Type");
				NotifyPropertyChanged("TypeStr");
			}
		}

		public string TypeStr
		{
			get { return _type.ToString().ToLower(); }
		}

		public IEnumerable<IntData> Ints
		{
			get { return _ints.Values; }
		}

		public ICommand CheckAllCommand { get; private set; }

		public ICommand UncheckAllCommand { get; private set; }

        public void DefineInt(string name, int offset, int count, bool signed)
		{
			var data = new IntData(this, name, offset, count, signed);
			_ints[offset] = data;
		}

		public override void Accept(IMetaFieldVisitor visitor)
		{
			visitor.VisitPackedInt(this);
		}

		public override MetaField CloneValue()
		{
			var result = new PackedIntData(Name, Offset, FieldAddress, _type, base.PluginLine);
			foreach (var @int in _ints)
				result.DefineInt(@int.Value.Name, @int.Value.Offset, @int.Value.Count, @int.Value.Singed);
			result.Value = _value;
			return result;
		}

		private void RefreshInts()
		{
			foreach (IntData @int in Ints)
				@int.Refresh();
		}
    }

	public class IntData : PropertyChangeNotifier
	{
		private readonly PackedIntData _parent;
        private string _name;
        private int _offset;
        private int _count;
        private bool _signed;

        public IntData(PackedIntData parent, string name, int offset, int count, bool signed)
		{
			_parent = parent;
			_name = name;
            _offset = offset;
            _count = count;
            _signed = signed;
		}

        public int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                NotifyPropertyChanged("Offset");
            }
        }

        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                NotifyPropertyChanged("Count");
            }
        }

        public bool Singed
        {
            get => _signed;
            set
            {
                _signed = value;
                NotifyPropertyChanged("Singed");
            }
        }

        public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				NotifyPropertyChanged("Name");
			}
		}

		public long Value
		{
            get {
                UInt32 bitmask = 0;
                for(var i = Offset;i<Offset+Count;i++)
                {
                    bitmask |= (UInt32)(1 << i);
                }

                UInt32 masked = _parent.Value & bitmask;
                UInt32 result = masked >> Offset;

                return result;
            }
            set
            {
                UInt32 _value = (UInt32)value;
                _value = _value << Offset;

                UInt32 bitmask = 0;
                for (var i = Offset; i < Offset + Count; i++)
                {
                    bitmask |= (UInt32)(1 << i);
                }
                _value = _value & bitmask;

                _parent.Value = _value | (_parent.Value & ~bitmask);

                // Changing the parent value causes a refresh,
                // so no need to call NotifyPropertyChanged
            }
		}

		public void Refresh()
		{
			NotifyPropertyChanged("Value");
		}
	}
}