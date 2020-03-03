using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using SciterSharp;

namespace UltraSidebar
{
	class AreaChart : ChartBase
	{
		private AreaSeries _series;
		private SciterElement _se;

		protected override void Attached(SciterElement se)
		{
			_se = se;
			_model = new PlotModel();

			var axisX = new LinearAxis()
			{
				Title = "R$",
				TickStyle = TickStyle.None,
				Position = AxisPosition.Left,
				MaximumPadding = 0.0,// removes padding inside view
				MinimumPadding = 0.0,
				Maximum = 5,
				MajorGridlineColor = OxyColor.Parse("#CDD2D6"),
				MajorGridlineStyle = LineStyle.Solid,
			};
			var axisY = new LinearAxis()
			{
				TickStyle = TickStyle.None,
				Position = AxisPosition.Bottom,
				MaximumPadding = 0.0,// removes padding inside view
				MinimumPadding = 0.0,
				TextColor = OxyColors.Transparent,
				MaximumRange = 300,
				MinimumRange = 300,
				MajorGridlineColor = OxyColor.Parse("#CDD2D6"),
				MajorGridlineStyle = LineStyle.Solid,
			};

			// Black coloring?
			bool black = se.Attributes.ContainsKey("black-style");
			if(black)
			{
				_model.Background = OxyColors.Black;
				_model.TextColor = OxyColors.White;
				_model.PlotAreaBorderColor = OxyColors.White;

				var c = OxyColors.White;
				axisX.MajorGridlineColor = OxyColor.FromAColor(40, c);
				axisX.MinorGridlineColor = OxyColor.FromAColor(20, c);
				axisX.MinorGridlineStyle = LineStyle.Solid;
				axisX.TicklineColor = OxyColors.White;
				axisY.MajorGridlineColor = OxyColor.FromAColor(40, c);
				axisY.MinorGridlineColor = OxyColor.FromAColor(20, c);
				axisY.MinorGridlineStyle = LineStyle.Solid;
				axisY.TicklineColor = OxyColors.White;
			}

			_model.Axes.Add(axisX);
			_model.Axes.Add(axisY);

			var fill = OxyColor.FromAColor(180, OxyColor.Parse("#E8DFAE"));
			_series = new AreaSeries()
			{
				Color = OxyColor.Parse("#EDC240"),
				StrokeThickness = 3,
			};
			_model.Series.Add(_series);
		}

		public void Host_SetCurrencyData(SciterValue[] args)
		{
			var items = new List<DataPoint>();
			_series.ItemsSource = items;
			_se.Refresh();
		}
	}
}