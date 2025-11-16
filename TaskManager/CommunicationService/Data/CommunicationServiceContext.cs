using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommunicationService.Models;

namespace CommunicationService.Data
{
    public class CommunicationServiceContext : DbContext
    {
        public CommunicationServiceContext (DbContextOptions<CommunicationServiceContext> options)
            : base(options)
        {
        }

        public DbSet<CommunicationService.Models.CommentEntity> Comments { get; set; } = default!;
    }
}
