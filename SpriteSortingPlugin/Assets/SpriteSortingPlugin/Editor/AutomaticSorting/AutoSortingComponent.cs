using System.Collections.Generic;

namespace SpriteSortingPlugin.AutomaticSorting
{
    public class AutoSortingComponent : SortingComponent
    {
        public int sortingOrder;
        public AutoSortingComponent containedByAutoSortingComponent;

        private List<SortingComponent> overlappingSortingComponents;

        public AutoSortingComponent(SortingComponent sortingComponent) : base(sortingComponent.OriginSpriteRenderer,
            sortingComponent.OutmostSortingGroup)
        {
            sortingOrder = OriginSortingOrder;
        }

        public void AddOverlappingSortingComponent(SortingComponent sortingComponent)
        {
            if (overlappingSortingComponents == null)
            {
                overlappingSortingComponents = new List<SortingComponent>();
            }

            overlappingSortingComponents.Add(sortingComponent);
        }

        public bool IsOverlapping(AutoSortingComponent autoSortingComponent)
        {
            if (overlappingSortingComponents == null)
            {
                return false;
            }

            foreach (var overlappingSortingComponent in overlappingSortingComponents)
            {
                if (overlappingSortingComponent.Equals(autoSortingComponent))
                {
                    return true;
                }
            }

            return false;
        }
    }
}