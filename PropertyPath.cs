// / ///////////////////////////////////////////////////////////////////////////////////////////////
// /
//  file:	PropertyPath.cs
// 
//  summary:	Implements functions for dealing with property paths in the "property[.child...]"
//  form. 
/// ////////////////////////////////////////////////////////////////////////////////////////////////

namespace Binding
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    
    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Implements functions for dealing with property paths in the "property[.child...]" form.
    /// </summary>
    ///
    /// <remarks>   Last edited by Ron, 12/27/2014. </remarks>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////
    internal static class PropertyPath
    {
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Takes an object and a property path and decends into the path until the first match is found,
        /// updating the object and path reference with the match.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/27/2014. </remarks>
        ///
        /// <param name="obj">          [in,out] The object the property path belongs to. </param>
        /// <param name="propertyPath"> [in,out] The property path given in property[.child....] form. </param>
        /// <param name="predicate">    Decent into the property path stops, when this predicate returns
        ///                             <c>true</c> or when the final child property is reached. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void FindChild(ref INotifyPropertyChanged obj, ref string propertyPath, Func<object, bool> predicate)
        {
            var path = propertyPath.Split('.');

            for (int i = 0; i < path.Count() - 1; i++)
            {
                var child = Property.GetReflected(obj, path[i]);

                if (predicate(child))
                {
                    propertyPath = String.Join(".", path.Skip(i));
                    return;
                }

                obj = (INotifyPropertyChanged)child;
            }
            propertyPath = path.Last();
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Finds the furthest object along the given property path that supports
        /// <see cref="System.ComponentModel.INotifyPropertyChanged"/> and updates the object and path to
        /// refer to it and its corresponding property.
        /// </summary>
        ///
        /// <remarks>   Last edited by Ron, 12/27/2014. </remarks>
        ///
        /// <param name="obj">          [in,out] The object the property path belongs to. </param>
        /// <param name="propertyPath"> [in,out] The property path in property[.child...] form. </param>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void FindLeafNotifiable(ref INotifyPropertyChanged obj, ref string propertyPath)
        {
            Func<object, bool> predicate = (o) => { return (o as INotifyPropertyChanged) == null; };

            FindChild(ref obj, ref propertyPath, predicate);
        }
    }
}
