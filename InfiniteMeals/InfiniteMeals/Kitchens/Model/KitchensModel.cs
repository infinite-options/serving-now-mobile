using System;
namespace InfiniteMeals.Kitchens.Model
{
    public class KitchensModel
    {
        public string kitchen_id { get; set; }
        public string title { get; set; }
        public string close_time { get; set; }
        public string description { get; set; }
        public string open_time { get; set; }
        public bool isOpen { get; set; }
        public string status { get; set; }
        public string statusColor { get; set; }
        public string opacity { get; set; }
    }
}
