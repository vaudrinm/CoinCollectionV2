using System;

namespace CoinCollection
{
    public class CollectionItem
    {
        public string Id { get; set; } = string.Empty;

        public string SubId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        // TODO: should be calculated field
        public double RetailValue { get { return UnitRetailValue * Count; } }

        // TODO: should be calculated field
        public double WholesaleValue { get { return UnitWholesaleValue * Count; } }

        public double UnitRetailValue { get; set; }

        public double UnitWholesaleValue { get; set; }

        public int? Year { get; set; }

        public int Count { get; set; }

        public string Description { get; set; } = string.Empty;

        public bool IsSummary { get; set; }

        public bool IsUnique { get; set; }

        public static ValueType ValueComparisonType { get; set; } = ValueType.Retail;

        public int GroupPreSort { get; set; }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is CollectionItem other)
            {
                return CompareTo(other);
            }

            throw new ArgumentException("A CollectionItem object is required for comparison.", "obj");
        }

        public int CompareTo(CollectionItem? other)
        {
            if (other is null)
            {
                return 1;
            }

            // Ratings compare opposite to normal string order, 
            // so reverse the value returned by String.CompareTo.
            //return -string.Compare(this.Rating, other.Rating, StringComparison.OrdinalIgnoreCase);

            if (ValueComparisonType == ValueType.Retail)
            {
                return this.RetailValue.CompareTo(other.RetailValue);
            }
            else
            {
                return this.WholesaleValue.CompareTo(other.WholesaleValue);
            }
        }

        public static int Compare(CollectionItem left, CollectionItem right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left is null)
            {
                return -1;
            }
            return left.CompareTo(right);
        }

        // Omitting Equals violates rule: OverrideMethodsOnComparableTypes.
        public override bool Equals(object? obj)
        {
            if (obj is CollectionItem other)
            {
                return this.CompareTo(other) == 0;
            }

            return false;
        }

        // Omitting getHashCode violates rule: OverrideGetHashCodeOnOverridingEquals.
        public override int GetHashCode()
        {
            //if (Id.Equals(string.Empty))
            //{
            //    throw new Exception("Id uninitialized");
            //}
            char[] c = this.Name.ToCharArray();
            return (int)c[0];
        }

        // Omitting any of the following operator overloads 
        // violates rule: OverrideMethodsOnComparableTypes.
        public static bool operator ==(CollectionItem left, CollectionItem right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(CollectionItem left, CollectionItem right)
        {
            return !(left == right);
        }
        public static bool operator <(CollectionItem left, CollectionItem right)
        {
            return (Compare(left, right) < 0);
        }
        public static bool operator >(CollectionItem left, CollectionItem right)
        {
            return (Compare(left, right) > 0);
        }

        public CollectionItem Clone()
        {
            return new CollectionItem
            {
                Id = this.Id,
                SubId = this.SubId,
                Name = this.Name,
                UnitRetailValue = this.UnitRetailValue,
                UnitWholesaleValue = this.UnitWholesaleValue,
                Year = this.Year,
                Count = this.Count,
                Description = this.Description,
                IsSummary = this.IsSummary,
                IsUnique = this.IsUnique,
                GroupPreSort = this.GroupPreSort,
            };
        }
    }
}
