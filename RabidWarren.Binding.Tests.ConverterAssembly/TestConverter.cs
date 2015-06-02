// -----------------------------------------------------------------------
//  <copyright file="TestConverter.cs" company="Ron Parker">
//   Copyright 2015 Ron Parker
//  </copyright>
//  <summary>
//   A converter for testing ConverterRegistry.RegisterAllConverters().
//  </summary>
// -----------------------------------------------------------------------

namespace RabidWarren.Binding.Tests.ConverterAssembly
{
    using System;

    /// <summary>
    /// This is for testing the ChickenAndEgg bug in ConverterRegistry.RegisterAllConverters.
    /// </summary>
    [BindingConverter(typeof(TestConverter), typeof(object))]
    public class TestConverter : BindingConverter
    {
        public override object ConvertTo(object value, Type targetType, object parameter)
        {
            throw new System.NotImplementedException();
        }

        public override object ConvertFrom(object value, Type targetType, object parameter)
        {
            throw new System.NotImplementedException();
        }
    }
}
