using System.Windows;
using System.Windows.Media;

namespace StarMeter.Models
{
    internal class ObjectFinder
    {
        // TAKEN FROM http://stackoverflow.com/questions/15184501/how-to-give-style-to-wpf-toolkit-chart

        /// <summary>
        /// Loop through all child items in the parent, looking for an object of the required type/name
        /// </summary>
        /// <typeparam name="T">The data type of the object being searched for</typeparam>
        /// <param name="parent">The container to search in</param>
        /// <param name="childName">The name of the child to find</param>
        /// <returns></returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            // loop through all children 
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var childType = child as T;
                if (childType == null)
                {
                    // recursive
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
    }
}