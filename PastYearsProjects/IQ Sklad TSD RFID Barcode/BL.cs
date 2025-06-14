using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using IQSoft.Core.BL;
using IQSoft.Utils;

namespace IQSoft.Core.BL
{
    public enum DbObjectType
    {
        Unknown,
        ArticleItem,
        StorageCell,
        Storage,
        Document
    }
    
    public class DbObject : INotifyPropertyChanged
    {
        [XmlAttribute]
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged("Title"); } }
        private string _title;

        [XmlAttribute]
        public string Guid { get { return _guid; } set { _guid = value; OnPropertyChanged("Guid"); } }
        private string _guid;
        
        [XmlIgnore]
        public DbObjectType Type { get; set; }

        [XmlIgnore]
        public int InternalId { get; set; }

        public string Attribute {
            get { return _attribute; }
            set { _attribute = value; OnPropertyChanged("Attribute"); } }
        private string _attribute;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [XmlRoot("Property")]
    public class CustomProperty
    {
        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}={1}", Key, Value);
        }

        public void Parse(string prop)
        {
            if (prop.Length > 0)
            {
                var firstEquPos = prop.IndexOf('=');
                if (firstEquPos > 0)
                {
                    var key = prop.Substring(0, firstEquPos);
                    var val = prop.Substring(firstEquPos + 1);
                    Key = key;
                    Value = val;
                }
            }
        }
    }

    public class CustomProperties : List<CustomProperty>
    {
        public bool ContainsKey(string key)
        {
            var prop = this.FirstOrDefault(p => p.Key == key);
            return prop != null;
        }

        public CustomProperty GetItemByValue(string value)
        {
            return this.FirstOrDefault(prop => prop.Value == value);
        }

        public string this[string key]
        {
            get
            {
                var prop = this.FirstOrDefault(p => p.Key == key);
                return prop != null ? prop.Value : "";
            }
            set
            {
                var prop = this.FirstOrDefault(p => p.Key == key);
                if (prop == null)
                {
                    prop = new CustomProperty {Key = key};
                    Add(prop);
                }
                prop.Value = value;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var prop in this)
            {
                sb.Append(prop.ToString());
                sb.Append("|nPrp|");
            }
            return sb.ToString();
        }

        public void ParseString(string stringFrom)
        {
            if (string.IsNullOrEmpty(stringFrom)) return;

            for (int i = 100; i < 30000; i++)
            {
                var chr = Convert.ToChar(i);
                if (!stringFrom.Contains(chr))
                {
                    var s = stringFrom.Replace("|nPrp|", chr.ToString());
                    var props = s.Split(chr);
                    foreach (var prop in props)
                    {
                        var cp = new CustomProperty();
                        cp.Parse(prop);
                        if (prop.Length >= 2) this[cp.Key] = cp.Value;
                    }
                    break;
                }
            }
        }
    }

    public class ArtilceItem : DbObject
    {
        public ArtilceItem()
        {
            Type = DbObjectType.ArticleItem;
        }

        [XmlArrayItem("Property")]
        public CustomProperties CustomProperties = new CustomProperties();
    }

    public class Cell : DbObject
    {
        public Cell()
        {
            Type = DbObjectType.StorageCell;
        }

        private string _parentStorageGuid;
        public string ParentStorageGuid
        {
            get { return _parentStorageGuid; }
            set { _parentStorageGuid = value; OnPropertyChanged("ParentStorageGuid"); }
        }
    }

    public enum QtyParity
    {
        Ok,
        NotEnough,
        TooMuch
    }

    [XmlRoot("Line")]
    public class OrderLine : INotifyPropertyChanged
    {
        [XmlIgnore]
        public OrderLines ParentTable;

        [XmlAttribute]
        public string Comment { get { return _comment; } set { _comment = value; OnPropertyChanged("Comment"); } }
        private string _comment;

        [XmlAttribute("Item")]
        public string ArticleItemGuid { get { return _articleItemGuid; } set { _articleItemGuid = value; OnPropertyChanged("ArticleItemGuid"); OnPropertyChanged("ItemTitle"); } }
        private string _articleItemGuid;

        [XmlAttribute("Cell")]
        [DefaultValue("iq_default_cell")] //TODO: не забыть, что эта волшебная строка должна быть равна Core.SAM.Database.DefaultCell.Guid
        public string CellGuid
        {
            get { return _cellGuid; }
            set
            {
                _cellGuid = value;
                OnPropertyChanged("CellGuid");
                OnPropertyChanged("CellTitle");
            }
        }

        private string _cellGuid;

        [XmlAttribute]
        [DefaultValue("")]
        public string EnteredCodes
        {
            get
            {
                var sb = new StringBuilder("");

                foreach (var code in _enteredCodes)
                {
                    sb.Append(code+" ");
                }
                
                return sb.ToString().TrimEnd(' ');
            }
            set
            {
                if (!_enteredCodes.Contains(value)) _enteredCodes.Add(value);
                OnPropertyChanged("EnteredCodes");
            }
        }

        private List<string> _enteredCodes=new List<string>();

        [XmlIgnore]
        public float QtyActual
        {
            get { return QtyActualString.IqToSingle(); }
            set
            {
                QtyActualString = value.ToString();
                OnPropertyChanged("QtyActual");
                OnPropertyChanged("QtyParity");
                OnPropertyChanged("QtyParityAsString");
            }
        }

        [XmlIgnore]
        public float QtyRequired
        {
            get { return QtyRequiredString.IqToSingle(); }
            set
            {
                QtyRequiredString = value.ToString();
                OnPropertyChanged("QtyRequired");
                OnPropertyChanged("QtyParity");
                OnPropertyChanged("QtyParityAsString");
            }
        }


        [XmlAttribute("QtyRequired")]
        public string QtyRequiredString { get; set; }

        [XmlAttribute("QtyActual")]
        public string QtyActualString { get; set; }


        /// <summary>
        /// Сравнивает требуемое и фактическое количества
        /// </summary>
        [XmlIgnore]
        public QtyParity QtyParity
        {
            get
            {
                var delta = QtyActual - QtyRequired;
                if (delta.IsAlmostZero()) return QtyParity.Ok;
                return delta > 0 ? QtyParity.TooMuch : QtyParity.NotEnough;
            }
        }

        [XmlIgnore]
        public string ParityAsString
        {
            get { return QtyParity.ToRusString(); }
        }

        [XmlIgnore]
        public string ItemTitle
        {
            get
            {
                var obj = IQSoft.Core.ObjectsAdapter.GetObjectFromDb(this.ArticleItemGuid);
                var t = obj.Title;
                if (obj.Type != DbObjectType.ArticleItem) t = "??? " + t;
                return t;
            }
        }

        [XmlIgnore]
        public string CellTitle
        {
            get
            {
                var obj = IQSoft.Core.ObjectsAdapter.GetObjectFromDb(this.CellGuid);
                var t = obj.Title;
                if (obj.Type != DbObjectType.StorageCell) t = "??? " + t;
                return t;
            }
        }

        public static OrderLine New(string itemGuid, string cellGuid, float actualQty)
        {
            var l = new OrderLine
                {
                    ArticleItemGuid = itemGuid,
                    CellGuid = cellGuid,
                    QtyActual = actualQty,
                    QtyRequired = 0
                };
            return l;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OrderLines : BindingList<OrderLine>
    {
        [XmlIgnore]
        public AbstractOrder ParentOrder { get; set; }
        
        /// <summary>
        /// Показывает, есть ли среди строчек хотя бы одна, в которой плановое кол-во не равно нулю.
        /// </summary>
        /// <returns></returns>
        public bool HasRequiredQty()
        {
            return this.Any(line => !line.QtyRequired.IsAlmostZero());
        }

        public bool IsAllLinesParityOk()
        {
            return this.All(line => line.QtyParity == QtyParity.Ok);
        }

        public new void Add(OrderLine line)
        {
            PushLine(line);
        }

        public OrderLine PushLine(OrderLine line)
        {
            OrderLine existingLine;

            if (ParentOrder.ConsiderCells)
                existingLine = this.FirstOrDefault(l => l.HasSameItemAndCell(line));
            else
                existingLine = this.FirstOrDefault(l => l.ArticleItemGuid == line.ArticleItemGuid);


            if (existingLine != null)
            {
                existingLine.QtyActual += line.QtyActual;
                line = existingLine;
            }
            else
            {
                line.ParentTable = this;
                base.Add(line);
            }

            return line;
        }

        public void GetTotalItemCountDisconsideringCells(string itemGuid, out float qtyActual, out float qtyRequired)
        {
            float qAct = 0;
            float qReq = 0;
            foreach (var line in this.Where(line => line.ArticleItemGuid == itemGuid))
            {
                qAct += line.QtyActual;
                qReq += line.QtyRequired;
            }
            qtyActual = qAct;
            qtyRequired = qReq;
        }
    }

    public abstract class Document : DbObject
    {
        protected Document()
        {
            Type = DbObjectType.Document;
        }

        [XmlAttribute]
        public string DocType { get; set; }

        [XmlAttribute]
        public string Date { get; set; }
    }

    public enum OrderType
    {
        Custom,
        Move,
        Sales,
        Purchase,
        Inventory
    }

    public abstract class AbstractOrder : Document
    {
        
        protected AbstractOrder()
        {
            
            Lines.ParentOrder = this;
        }

        [XmlIgnore]
        public string FileName { get; set; }

        [XmlIgnore]
        public OrderType Type { get; protected set; }

        public OrderLines Lines = new OrderLines();

        /// <summary>
        /// Показывает, есть ли среди строчек хотя бы одна, в которой плановое кол-во не равно нулю.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasRequiredQty()
        {
            return Lines.HasRequiredQty();
        }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool ConsiderCells { get; set; }

        public virtual bool GetQtyParity()
        {
            return !HasRequiredQty() || Lines.IsAllLinesParityOk();
        }

        [XmlIgnore]
        public bool AcceptUnknownItems { get; protected set; }
    }

    [XmlRoot("Order")]
    public class InventoryOrder : AbstractOrder
    {
        public InventoryOrder()
        {
            DocType = "inventory";
            Type = OrderType.Inventory;
            AcceptUnknownItems = false;
        }
    }

    [XmlRoot("Order")]
    public class CustomOrder : AbstractOrder
    {
        public CustomOrder()
        {
            DocType = "custom";
            Type = OrderType.Custom;
            AcceptUnknownItems = true;
        }
    }

    [XmlRoot("Order")]
    public class PurchaseOrder : AbstractOrder
    {
        public PurchaseOrder()
        {
            DocType = "purchase";
            Type = OrderType.Purchase;
            AcceptUnknownItems = true;
        }
    }

    [XmlRoot("Order")]
    public class SalesOrder : AbstractOrder
    {
        public SalesOrder()
        {
            DocType = "sales";
            Type = OrderType.Sales;
            AcceptUnknownItems = false;
        }
    }

    [XmlRoot("Order")]
    public class MoveOrder : AbstractOrder
    {
        public MoveOrder()
        {
            DocType = "move";
            Type = OrderType.Move;
            AcceptUnknownItems = false;
            Lines2.ParentOrder = this;
        }
        
        public OrderLines Lines2 = new OrderLines();

        public override bool HasRequiredQty()
        {
            return base.HasRequiredQty() || Lines2.HasRequiredQty();
        }

        public override bool GetQtyParity()
        {
            if (Lines.HasRequiredQty())
                if (!Lines.IsAllLinesParityOk()) return false;

            if (Lines2.HasRequiredQty())
                if (!Lines2.IsAllLinesParityOk()) return false;

            var items = Lines.Select(line => line.ArticleItemGuid).Distinct();

            foreach (var guid in items)
            {
                float qtyAct;
                float qtyReq;
                Lines.GetTotalItemCountDisconsideringCells(guid, out qtyAct, out qtyReq);

                float qtyAct2;
                float qtyReq2;
                Lines2.GetTotalItemCountDisconsideringCells(guid, out qtyAct2, out qtyReq2);

                if (!qtyAct.IsAlmostEqualTo(qtyAct2)) return false;
            }

            return true;
        }
    }

    public static class OrderLinesExtensions
    {
        public static bool HasSameItemAndCell(this OrderLine thisLine, OrderLine otherLine)
        {
            return (thisLine.ArticleItemGuid == otherLine.ArticleItemGuid) && (thisLine.CellGuid == otherLine.CellGuid);
        }
        
        public static string ToRusString(this QtyParity qtyParity)
        {
            string s;
            switch (qtyParity)
            {
                case QtyParity.Ok:
                    s = @"OK";
                    break;
                case QtyParity.NotEnough:
                    s = @"Мало";
                    break;
                case QtyParity.TooMuch:
                    s = @"Много";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("qtyParity");
            }
            return s;
        }

        public static string ToRusString(this OrderType type)
        {
            string s = "???";
            switch (type)
            {
                case OrderType.Custom:
                    s = "Сбор кодов";
                    break;
                case OrderType.Move:
                    s = "Перемещение";
                    break;
                case OrderType.Sales:
                    s = "Реализация";
                    break;
                case OrderType.Purchase:
                    s = "Приемка";
                    break;
                case OrderType.Inventory:
                    s = "Инвентаризация";
                    break;
            }
            return s;
        }
    }

    public static class StaticStringConverter
    {
        public static float IqToSingle(this string s)
        {
            float r = 0;
            if (!string.IsNullOrEmpty(s))
            {
                s = s.Replace('.', '*');
                s = s.Replace(',', '*');
                var c = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
                s = s.Replace("*", c);
                r = Convert.ToSingle(s);
            }
            return r;
        }
    }

}
