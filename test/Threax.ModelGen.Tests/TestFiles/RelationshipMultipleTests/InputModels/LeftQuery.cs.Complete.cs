using Halcyon.HAL.Attributes;
using Test.Controllers;
using Test.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threax.AspNetCore.Halcyon.Ext;
using Threax.AspNetCore.Halcyon.Ext.ValueProviders;
using Threax.AspNetCore.Models;
using System.ComponentModel.DataAnnotations;

namespace Test.InputModels
{
    [HalModel]
    public partial class LeftQuery : PagedCollectionQuery
    {
        /// <summary>
        /// Lookup a left by id.
        /// </summary>
        public Guid? LeftId { get; set; }

        /// <summary>
        /// Populate an IQueryable. Does not apply the skip or limit.
        /// </summary>
        /// <param name="query">The query to populate.</param>
        /// <returns>The query passed in populated with additional conditions.</returns>
        public Task<IQueryable<LeftEntity>> Create(IQueryable<LeftEntity> query)
        {
            if (LeftId != null)
            {
                query = query.Where(i => i.LeftId == LeftId);
            }
            else
            {
                //Customize query further
            }

            return Task.FromResult(query);
        }
    }
}