using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class StudentDeactivateTests
{
    [Fact]
    public async Task Given_ActiveStudents_When_DeactivatingBatch_Then_AllBecomeInactive()
    {
        var repo = new FakeStudentRepository().WithSection(1,
            new Student { Id = 1, LastName = "A", FirstName = "Uno", StudentNumber = "1", IsActive = true },
            new Student { Id = 2, LastName = "B", FirstName = "Dos", StudentNumber = "2", IsActive = true },
            new Student { Id = 3, LastName = "C", FirstName = "Tres", StudentNumber = "3", IsActive = true });
        var service = new StudentService(repo);

        await service.DeactivateAsync([1, 3]);

        Assert.False(repo.GetById(1)!.IsActive);
        Assert.True(repo.GetById(2)!.IsActive);
        Assert.False(repo.GetById(3)!.IsActive);
    }

    [Fact]
    public async Task Given_EmptyList_When_Deactivating_Then_NoChanges()
    {
        var repo = new FakeStudentRepository().WithSection(1,
            new Student { Id = 1, LastName = "A", FirstName = "Uno", StudentNumber = "1", IsActive = true });
        var service = new StudentService(repo);

        await service.DeactivateAsync([]);

        Assert.True(repo.GetById(1)!.IsActive);
    }

    [Fact]
    public async Task Given_RepositoryFailure_When_Deactivating_Then_PropagatesError()
    {
        var repo = new FakeStudentRepository().WithSection(1,
            new Student { Id = 1, LastName = "A", FirstName = "Uno", StudentNumber = "1", IsActive = true });
        repo.FailNextDeactivate = true;
        var service = new StudentService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeactivateAsync([1]));
        Assert.True(repo.GetById(1)!.IsActive);
    }
}
