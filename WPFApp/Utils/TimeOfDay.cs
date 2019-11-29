using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFApp.Utils
{
	public class TimeOfDay
	{
		public static DateTime Today = DateTime.Today;
		public static DateTime Tomorrow = DateTime.Today.AddDays(1);
		public static DateTime AfterTomorrow = DateTime.Today.AddDays(2);
		public static DateTime Midday = DateTime.Parse("12:00:00");	// highest temp 
		public static DateTime Midnight = DateTime.Parse("00:00:00"); // lowest temp 
		public static DateTime SixAM = DateTime.Parse("09:00:00");
		public static DateTime SixPM = DateTime.Parse("18:00:00");
		public static DateTime NineAM = DateTime.Parse("09:00:00");
		public static DateTime NinePM = DateTime.Parse("21:00:00");
	}
}
