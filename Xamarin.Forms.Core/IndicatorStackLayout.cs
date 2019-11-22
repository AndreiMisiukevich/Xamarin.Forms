﻿using System.Collections;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	internal class IndicatorStackLayout : StackLayout
	{
		IndicatorView _indicatorView;
		public IndicatorStackLayout(IndicatorView indicatorView)
		{
			_indicatorView = indicatorView;
			Orientation = StackOrientation.Horizontal;
			_indicatorView.PropertyChanged += _indicatorViewPropertyChanged;
		}

		void _indicatorViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == IndicatorView.IndicatorsShapeProperty.PropertyName
				|| e.PropertyName == IndicatorView.IndicatorTemplateProperty.PropertyName)
			{
				ResetIndicators();
			}
			if (e.PropertyName == IndicatorView.MaximumVisibleProperty.PropertyName
				|| e.PropertyName == IndicatorView.PositionProperty.PropertyName
				|| e.PropertyName == IndicatorView.HideSingleProperty.PropertyName
				|| e.PropertyName == IndicatorView.IndicatorColorProperty.PropertyName
				|| e.PropertyName == IndicatorView.SelectedIndicatorColorProperty.PropertyName
				|| e.PropertyName == IndicatorView.IndicatorSizeProperty.PropertyName)
			{
				ResetIndicatorStyles();
			}
		}

		void ResetIndicatorStyles()
		{
			try
			{
				BatchBegin();
				ResetIndicatorStylesNonBatch();
			}
			finally
			{
				BatchCommit();
			}
		}

		void ResetIndicators()
		{
			try
			{
				BatchBegin();
				Children.Clear();
				AddExtraIndicatorItems();
			}
			finally
			{
				ResetIndicatorStylesNonBatch();
				BatchCommit();
			}
		}

		internal void ResetIndicatorCount(int oldCount)
		{
			try
			{
				BatchBegin();
				if (oldCount < 0)
				{
					oldCount = 0;
				}

				if (oldCount > _indicatorView.Count)
				{
					RemoveRedundantIndicatorItems();
					return;
				}

				AddExtraIndicatorItems();
			}
			finally
			{
				ResetIndicatorStylesNonBatch();
				BatchCommit();
			}
		}

		void ResetIndicatorStylesNonBatch()
		{
			var indicatorCount = _indicatorView.Count;
			var childrenCount = Children.Count;

			for (int index = 0; index < childrenCount; index++)
			{
				Children[index].BackgroundColor = index == _indicatorView.Position
					? GetColorOrDefault(_indicatorView.SelectedIndicatorColor, Color.Gray)
					: GetColorOrDefault(_indicatorView.IndicatorColor, Color.Silver);
			}

			IsVisible = indicatorCount > 1 || !_indicatorView.HideSingle;
		}

		Color GetColorOrDefault(Color color, Color defaultColor)
			=> color.IsDefault ? defaultColor : color;

		internal void ResetItemsSource(IEnumerable oldItemsSource)
		{
			if (oldItemsSource is INotifyCollectionChanged oldCollection)
				oldCollection.CollectionChanged -= OnCollectionChanged;

			if (_indicatorView.ItemsSource is INotifyCollectionChanged collection)
				collection.CollectionChanged += OnCollectionChanged;

			OnCollectionChanged(_indicatorView.ItemsSource, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		void AddExtraIndicatorItems()
		{
			var indicatorCount = _indicatorView.Count;
			var indicatorMaximumVisible = _indicatorView.MaximumVisible;
			var indicatorSize = _indicatorView.IndicatorSize;
			var indicatorTemplate = _indicatorView.IndicatorTemplate;

			var oldCount = Children.Count;
			for (var i = 0; i < indicatorCount - oldCount && i < indicatorMaximumVisible - oldCount; i++)
			{
				var size = indicatorSize > 0 ? indicatorSize : 10;
				var indicator = indicatorTemplate?.CreateContent() as View ?? new Frame
				{
					Padding = 0,
					HasShadow = false,
					BorderColor = Color.Transparent,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					WidthRequest = size,
					HeightRequest = size,
					CornerRadius = _indicatorView.IndicatorsShape == IndicatorShape.Circle ? (float)size / 2 : 0
				};
				var tapGestureRecognizer = new TapGestureRecognizer();
				tapGestureRecognizer.Tapped += (sender, args) => _indicatorView.Position = Children.IndexOf(sender as View);
				indicator.GestureRecognizers.Add(tapGestureRecognizer);
				Children.Add(indicator);
			}
		}

		void RemoveRedundantIndicatorItems()
		{
			var indicatorCount = _indicatorView.Count;
			while (Children.Count > indicatorCount)
			{
				Children.RemoveAt(0);
			}
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (sender is ICollection collection)
			{
				_indicatorView.Count = collection.Count;
				return;
			}
			var count = 0;
			var enumerator = (sender as IEnumerable)?.GetEnumerator();
			while (enumerator?.MoveNext() ?? false)
			{
				count++;
			}
			_indicatorView.Count = count;
		}
	}
}