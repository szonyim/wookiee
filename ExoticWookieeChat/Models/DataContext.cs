using ExoticWookieeChat.Constants;
using ExoticWookieeChat.Util;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ExoticWookieeChat.Models
{
    public class DataContext : DbContext
    {

        public DataContext() : base("EWCConnection")
        {
            Database.SetInitializer(new EWCDBInitializer());
        }

        /// <summary>
        /// Create new DataContext
        /// </summary>
        /// <returns></returns>
        public static DataContext CreateContext()
        {
            return new DataContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add<OneToManyCascadeDeleteConvention>();

            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Conversation> Conversations { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
    }

    /// <summary>
    /// Initalize database. Add first employee, who can add other employee
    /// </summary>
    //public class EWCDBInitializer : DropCreateDatabaseIfModelChanges<DataContext>
    public class EWCDBInitializer: CreateDatabaseIfNotExists<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            User firstEmp = new User()
            {
                DisplayName = "Admin",
                UserName = "admin",
                Password = PasswordUtil.GenerateSHA512String("admin"),
                CreatedAt = DateTime.Now,
                Role = UserRoleConstants.ROLE_ADMIN + ","  + UserRoleConstants.ROLE_EMPLOYEE
            };

            context.Users.Add(firstEmp);

            base.Seed(context);
        }
    }
}