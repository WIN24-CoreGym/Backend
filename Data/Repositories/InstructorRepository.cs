using Data.Contexts;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;
public class InstructorRepository(DataContext context) : BaseRepository<InstructorEntity>(context), IInstructorRepository
{

}
