﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Data;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter
{
	public class DynamicSingleConverter : IValueConverter
	{
		private static Dictionary<Type, Func<object, object>> castFunctions = new Dictionary<Type, Func<object, object>>();

		private Func<object, object[], object> _converter;
		private Func<object, object[], object> _convertBack;
		private object[] _toValues;
		private object[] _fromValues;
		public DynamicSingleConverter(Func<object, object[], object> converter, Func<object, object[], object> convertBack, object[] toValues, object[] fromValues)
		{
			_converter = converter;
			_convertBack = convertBack;
			_toValues = toValues;
			_fromValues = fromValues;
		}

		private object DoConversion(object value, Type targetType, Func<object, object[], object> func, object[] values)
		{
			object result = value;
			if (func != null)
			{
				try { result = func(result, values); }
				catch { return null; }
			}

			if (result == null)
				return null;

			if (targetType == typeof(string))
				return result.ToString();

			Func<object, object> cast;
			if (!castFunctions.TryGetValue(targetType, out cast))
			{
				ParameterExpression par = Expression.Parameter(typeof(object));
				cast = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Dynamic(Binder.Convert(CSharpBinderFlags.None, targetType, typeof(object)), targetType, par), typeof(object)), par).Compile();
				castFunctions.Add(targetType, cast);
			}

			result = cast(result);
			return result;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return DoConversion(value, targetType, _converter, _toValues);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return DoConversion(value, targetType, _convertBack, _fromValues);
		}
	}
}
