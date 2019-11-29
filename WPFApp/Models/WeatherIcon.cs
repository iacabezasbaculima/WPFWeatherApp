using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFApp.Models
{
	public class WeatherIcon
	{
		public Dictionary<int, string> iconList = new Dictionary<int, string>();
		public int IconId { get; set; }
		public string ImageId { get; set; }

		private void Init()
		{
			// RAIN: 5xx
			iconList.Add(500, "10d");
			iconList.Add(501, "10d");
			iconList.Add(502, "10d");
			iconList.Add(503, "10d");
			iconList.Add(504, "10d");
			iconList.Add(511, "13d");
			iconList.Add(520, "09d");
			iconList.Add(521, "09d");
			iconList.Add(522, "09d");
			iconList.Add(531, "09d");

			// SNOW: 6xx
			iconList.Add(600, "13d");
			iconList.Add(601, "13d");
			iconList.Add(602, "13d");
			iconList.Add(611, "13d");
			iconList.Add(612, "13d");
			iconList.Add(613, "13d");
			iconList.Add(615, "13d");
			iconList.Add(616, "13d");
			iconList.Add(620, "13d");
			iconList.Add(622, "13d");

		}
	}
}
