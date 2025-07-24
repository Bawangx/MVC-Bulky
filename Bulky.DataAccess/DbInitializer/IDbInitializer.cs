using Bulky.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInitializer
{
    public interface IDbInitializer 
    {
        void Initialize();
        // This method is used to seed the database with initial data.
        // It can be called during application startup to ensure that the database is ready for use.
        // The implementation should handle creating roles, users, and any other necessary data.    
    }
}