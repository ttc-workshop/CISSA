namespace Intersoft.CISSA.UserApp.Models
{
    public class ComboBoxItem
    {
        public string Value { get; private set; }
        public string Text { get; private set; }

        public ComboBoxItem(string value, string text)
        {
            Value = value;
            Text = text;
        }
    }

    public class ComboBoxUpdateData
    {
        public string id { get; private set; }
        public string value { get; private set; }

        public ComboBoxItem[] items { get; set; }

        public ComboBoxUpdateData(string id, string value)
        {
            this.id = id;
            this.value = value;
        }
    }
}