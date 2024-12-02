using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class PartNumberVehicleRepository(CleanArchBaseContext context) : BaseRepository<PartNumberVehicle>(context), IPartNumberVehicleRepository
    {
    }
}
