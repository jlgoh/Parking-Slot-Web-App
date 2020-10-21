using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Services
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping <TSource, TDestination>();
    }

    public class PropertyMappingService :IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _userPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "FirstName", new PropertyMappingValue(new List<string>() { "FirstName" } ) },
                { "LastName", new PropertyMappingValue(new List<string>() { "LastName" } ) },
                { "Email", new PropertyMappingValue(new List<string>() { "Email"} ) },
                { "Username", new PropertyMappingValue(new List<string>() { "Username" } ) },
                { "Topic", new PropertyMappingValue(new List<string>() { "Topic" }) },
            };
        private Dictionary<string, PropertyMappingValue> _feedbackPropertyMapping =
             new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
             {
                { "Topic", new PropertyMappingValue(new List<string>() { "Topic" }) }
             };

        private Dictionary<string, PropertyMappingValue> _carparkPropertyMapping =
             new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
             {
                { "CarparkName", new PropertyMappingValue(new List<string>() { "CarparkName" }) }
             };

        public IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<UserDto, User>(_userPropertyMapping));
            propertyMappings.Add(new PropertyMapping<FeedbackDto, Feedback>(_feedbackPropertyMapping));
            propertyMappings.Add(new PropertyMapping<CarparkDto, Carpark>(_carparkPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping
            <TSource, TDestination>()
        {
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}>");
        }
    }
}
