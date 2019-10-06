using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using InfiniteMeals.Kitchens.Model;
using System.Collections.ObjectModel;

namespace InfiniteMeals
{
    public partial class MainPage : ContentPage
    {

        ObservableCollection<KitchensModel> Kitchens = new ObservableCollection<KitchensModel>();

        protected async void GetKitchens()
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://phaqvwjbw6.execute-api.us-west-1.amazonaws.com/dev/api/v1/kitchens");
            request.Method = HttpMethod.Get;
            var client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                HttpContent content = response.Content;
                var kitchensString = await content.ReadAsStringAsync();
                JObject kitchens = JObject.Parse(kitchensString);
                this.Kitchens.Clear();

                foreach (var k in kitchens["result"])
                {
                    int dayOfWeekIndex = getDayOfWeekIndex(DateTime.Today);
                    //  Format from AM/PM to 24-hour format
                    //string start_time = DateTime.Parse((string)k["open_time"]["S"]).ToString("HH:mm");
                    //string end_time = DateTime.Parse((string)k["close_time"]["S"]).ToString("HH:mm");
                    string start_time = (string)k["accepting_hours"]["L"][dayOfWeekIndex]["M"]["open_time"]["S"];
                    string end_time = (string)k["accepting_hours"]["L"][dayOfWeekIndex]["M"]["close_time"]["S"];
                    //  Check if business is open for this day of the week
                    Boolean isAccepting = (Boolean)k["accepting_hours"]["L"][dayOfWeekIndex]["M"]["is_accepting"]["BOOL"];
                    //  Overall, is the business open?
                    Boolean businessIsOpen = isBusinessOpen(TimeSpan.Parse(start_time), TimeSpan.Parse(end_time), isAccepting);
                    //Boolean businessIsOpen = (Boolean)k["isOpen"]["BOOL"];
                    this.Kitchens.Add(new KitchensModel()
                    {
                        kitchen_id = k["kitchen_id"]["S"].ToString(),
                        title = k["kitchen_name"]["S"].ToString(),
                        close_time = /*k["close_time"]["S"].ToString()*/end_time,
                        description = k["description"]["S"].ToString(),
                        open_time = /*k["open_time"]["S"].ToString()*/start_time,
                        isOpen = businessIsOpen,
                        status = (businessIsOpen == true) ? "Open now" : "Closed",
                        statusColor = (businessIsOpen == true) ? "Green" : "Red",
                        opacity = (businessIsOpen == true) ? "1.0" : "0.6"
                    }
                    );
                }

                kitchensListView.ItemsSource = Kitchens;
            }

        }

        public MainPage()
        {
            InitializeComponent();

            GetKitchens();

            //Kitchens.Clear();

            kitchensListView.RefreshCommand = new Command(() =>
            {
               GetKitchens();
               kitchensListView.IsRefreshing = false;
            });

            kitchensListView.ItemSelected += Handle_ItemTapped();

        }

        private EventHandler<SelectedItemChangedEventArgs> Handle_ItemTapped()
        {
            return OnItemSelected;
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // disable selected item highlighting;
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;


            // do something with the selection
            var kitchen = e.SelectedItem as KitchensModel;

            // disable selection if the kitchen is closed
            if (kitchen.isOpen == false)
            {
                return;
            }

            await Navigation.PushAsync(new SelectMealOptions(kitchen.kitchen_id));
        }

        
        private int getDayOfWeekIndex(DateTime day)
        {
            if (day.DayOfWeek == DayOfWeek.Sunday)
                return 0;
            if (day.DayOfWeek == DayOfWeek.Monday)
                return 1;
            if (day.DayOfWeek == DayOfWeek.Tuesday)
                return 2;
            if (day.DayOfWeek == DayOfWeek.Wednesday)
                return 3;
            if (day.DayOfWeek == DayOfWeek.Thursday)
                return 4;
            if (day.DayOfWeek == DayOfWeek.Friday)
                return 5;
            if (day.DayOfWeek == DayOfWeek.Saturday)
                return 6;
            return -1;
        }

        //  Function checking if business is currently open
        private Boolean isBusinessOpen(TimeSpan open_time, TimeSpan close_time, Boolean is_accepting)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            //  Accepting orders on current day?
            if (is_accepting == false)
            {
                return false;
            }
            else
            {
                //  Opening and closing hours on same day
                if (open_time <= close_time)
                {
                    if (now >= open_time && now <= close_time)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                //  Opening and closing hours on different day
                else
                {
                    if (now >= open_time || now <= close_time)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
