using Microsoft.Extensions.DependencyInjection;
using MultipleDbContextsTransaction;
using MultipleDbContextsTransaction.Contexts;
using MultipleDbContextsTransaction.Entities;
using MultipleDbContextsTransaction.Extensions;

const string connectionString = "Data Source=.;Initial Catalog=MultipleDbContexts;Connection Timeout=5;User Id=sa;Password=P@ssword1;MultipleActiveResultSets=False;Timeout=5;TrustServerCertificate=true;";

var services = new ServiceCollection();

services.AddDbConnectionProvider(connectionString);

services.AddUnitOfWork();

services.AddDbContextWithSharedTransaction<My1DbContext>(o => new My1DbContext(o));
services.AddDbContextWithSharedTransaction<My2DbContext>(o => new My2DbContext(o));

var provider = services.BuildServiceProvider(true);

////////////////////////////////////////////////////////////////////////////////////////

using (var scope = provider.CreateScope())
{
    var sp = scope.ServiceProvider;

    var uow = sp.GetRequiredService<IUnitOfWork>();
    var context1 = sp.GetRequiredService<IUnitOfWork<My1DbContext>>();
    var context2 = sp.GetRequiredService<IUnitOfWork<My2DbContext>>();

    await uow.BeginTransactionAsync();

    context1.DataContext.Set<Tab1>().Add(new Tab1());
    context2.DataContext.Set<Tab2>().Add(new Tab2());

    await uow.CommitTransactionAsync();

    await uow.BeginTransactionAsync();

    context1.DataContext.Set<Tab1>().Add(new Tab1());
    await uow.SaveChangesAsync();
    context2.DataContext.Set<Tab2>().Add(new Tab2());

    await uow.CommitTransactionAsync();
}

Console.WriteLine("Finished");