using Data.Contexts;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;
public class AppUserRepository(DataContext context) : BaseRepository<AppUser>(context), IAppUserRepository
{

}
