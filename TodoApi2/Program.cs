using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//DI(���Ӽ� ����) �����̳ʿ� �����ͺ��̽� ���ؽ�Ʈ�� �߰��ϰ� �����ͺ��̽� ���� ���ܸ� ǥ��.
//DI �����̳ʴ� �����ͺ��̽� ���ؽ�Ʈ �� ��Ÿ ���񽺿� ���� �׼����� ����.
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));

var app = builder.Build();

//MapGet�� ���� ȣ���� ����Ͽ� ���� GET ��������Ʈ�� ����.
app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.Select(x => new TodoItemDTO(x)).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(new TodoItemDTO(todo))
            : Results.NotFound());

//MapPost�� ����Ͽ� �޸� �� �����ͺ��̽��� �����͸� �߰�.
app.MapPost("/todoitems", async (TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
});

//MapPut�� ����Ͽ� ���� PUT ��������Ʈ�� ����. �������� ������ 204(������ ����)�� ��ȯ.
app.MapPut("/todoitems/{id}", async (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

//MapDelete�� ����Ͽ� ���� DELETE ��������Ʈ�� ����.
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(new TodoItemDTO(todo));
    }

    return Results.NotFound();
});

//����.
app.Run();

//���� �ۿ��� �����ϴ� �����͸� ��Ÿ���� �Ϸ��� Ŭ����.
public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }
}

//DTO ��.
public class TodoItemDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public TodoItemDTO() { }
    public TodoItemDTO(Todo todoItem) =>
    (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}

//�����ͺ��̽� ���ؽ�Ʈ�� ������ �𵨿� �°� Entity Framework ����� �����ϴ� �� Ŭ����.
class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}