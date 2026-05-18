namespace TaskFlowAPI.Exceptions
{
    // thrown when a requirested resource does not exist
    // middleware catches this and returns 404
    public class NotFoundException:Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string entityName, int id):base($"{entityName} with id {id} was not found.") { }
    }
}
