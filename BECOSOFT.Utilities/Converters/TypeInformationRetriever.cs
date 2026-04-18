using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Converters {
    public static class TypeInformationRetriever<T> {
        public static readonly Func<TypeInformation> Instance = Create();

        private static Func<TypeInformation> Create() {
            var type = typeof(T);
            var typeInformation = type.GetTypeInformation();
            return Expression.Lambda<Func<TypeInformation>>(Expression.Constant(typeInformation), null).Compile();
        }
    }
}