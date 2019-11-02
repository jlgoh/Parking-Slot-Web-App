﻿using ParkingSlotAPI.Entities;
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
                { "Username", new PropertyMappingValue(new List<string>() { "Username" } ) }
            };

        public IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<UserDto, User>(_userPropertyMapping));
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
