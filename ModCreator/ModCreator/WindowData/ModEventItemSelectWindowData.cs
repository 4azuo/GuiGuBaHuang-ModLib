using ModCreator.Attributes;
using ModCreator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModCreator.WindowData
{
    /// <summary>
    /// Window data for ModEvent item selection (Events, Conditions, Actions)
    /// </summary>
    public class ModEventItemSelectWindowData : CWindowData
    {
        /// <summary>
        /// Window title
        /// </summary>
        public string WindowTitle { get; set; } = "Select Item";

        /// <summary>
        /// Type of items: "Event", "Condition", or "Action"
        /// </summary>
        public string ItemType { get; set; }

        /// <summary>
        /// Available categories
        /// </summary>
        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Selected category
        /// </summary>
        [NotifyMethod(nameof(OnCategoryChanged))]
        public string SelectedCategory { get; set; }

        /// <summary>
        /// Search text
        /// </summary>
        [NotifyMethod(nameof(OnSearchTextChanged))]
        public string SearchText { get; set; } = string.Empty;

        /// <summary>
        /// All available items
        /// </summary>
        public List<object> AllItems { get; set; } = new List<object>();

        /// <summary>
        /// Filtered items based on category and search
        /// </summary>
        public ObservableCollection<object> FilteredItems { get; set; } = new ObservableCollection<object>();

        /// <summary>
        /// Selected item
        /// </summary>
        public object SelectedItem { get; set; }

        /// <summary>
        /// Check if an item is selected
        /// </summary>
        public bool HasSelectedItem => SelectedItem != null;

        /// <summary>
        /// Initialize with events
        /// </summary>
        public void InitializeWithEvents(List<EventCategory> eventCategories)
        {
            ItemType = "Event";
            WindowTitle = "Select Event";
            
            Categories.Clear();
            Categories.Add("All");
            foreach (var cat in eventCategories.Select(c => c.Category).Distinct().OrderBy(c => c))
            {
                Categories.Add(cat);
            }
            
            AllItems.Clear();
            foreach (var category in eventCategories)
            {
                foreach (var evt in category.Events)
                {
                    AllItems.Add(new EventInfoDisplay
                    {
                        Category = category.Category,
                        Name = evt.Name,
                        DisplayName = evt.DisplayName,
                        Description = evt.Description,
                        Code = evt.Signature,
                        EventInfo = evt
                    });
                }
            }
            
            SelectedCategory = "All";
            UpdateFilteredItems();
        }

        /// <summary>
        /// Initialize with conditions
        /// </summary>
        public void InitializeWithConditions(List<ConditionInfo> conditions)
        {
            ItemType = "Condition";
            WindowTitle = "Select Condition";
            
            Categories.Clear();
            Categories.Add("All");
            foreach (var cat in conditions.Select(c => c.Category).Distinct().OrderBy(c => c))
            {
                Categories.Add(cat);
            }
            
            AllItems = conditions.Cast<object>().ToList();
            
            SelectedCategory = "All";
            UpdateFilteredItems();
        }

        /// <summary>
        /// Initialize with actions
        /// </summary>
        public void InitializeWithActions(List<ActionInfo> actions)
        {
            ItemType = "Action";
            WindowTitle = "Select Action";
            
            Categories.Clear();
            Categories.Add("All");
            foreach (var cat in actions.Select(a => a.Category).Distinct().OrderBy(c => c))
            {
                Categories.Add(cat);
            }
            
            AllItems = actions.Cast<object>().ToList();
            
            SelectedCategory = "All";
            UpdateFilteredItems();
        }

        /// <summary>
        /// Called when category selection changes
        /// </summary>
        public void OnCategoryChanged(object obj, System.Reflection.PropertyInfo prop, object oldValue, object newValue)
        {
            UpdateFilteredItems();
        }

        /// <summary>
        /// Called when search text changes
        /// </summary>
        public void OnSearchTextChanged(object obj, System.Reflection.PropertyInfo prop, object oldValue, object newValue)
        {
            UpdateFilteredItems();
        }

        /// <summary>
        /// Update filtered items based on category and search text
        /// </summary>
        private void UpdateFilteredItems()
        {
            FilteredItems.Clear();

            var query = AllItems.AsEnumerable();

            // Filter by category
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                query = query.Where(item =>
                {
                    if (item is EventInfoDisplay evt) return evt.Category == SelectedCategory;
                    if (item is ConditionInfo cond) return cond.Category == SelectedCategory;
                    if (item is ActionInfo act) return act.Category == SelectedCategory;
                    return false;
                });
            }

            // Filter by search text
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                query = query.Where(item =>
                {
                    if (item is EventInfoDisplay evt)
                        return evt.DisplayName?.ToLower().Contains(searchLower) == true ||
                               evt.Description?.ToLower().Contains(searchLower) == true ||
                               evt.Name?.ToLower().Contains(searchLower) == true;
                    if (item is ConditionInfo cond)
                        return cond.DisplayName?.ToLower().Contains(searchLower) == true ||
                               cond.Description?.ToLower().Contains(searchLower) == true ||
                               cond.Name?.ToLower().Contains(searchLower) == true;
                    if (item is ActionInfo act)
                        return act.DisplayName?.ToLower().Contains(searchLower) == true ||
                               act.Description?.ToLower().Contains(searchLower) == true ||
                               act.Name?.ToLower().Contains(searchLower) == true;
                    return false;
                });
            }

            foreach (var item in query)
            {
                FilteredItems.Add(item);
            }

            // Auto-select first item if only one result
            if (FilteredItems.Count == 1)
            {
                SelectedItem = FilteredItems[0];
            }
        }
    }

    /// <summary>
    /// Display wrapper for EventInfo to include category
    /// </summary>
    public class EventInfoDisplay
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public EventInfo EventInfo { get; set; }
    }
}
