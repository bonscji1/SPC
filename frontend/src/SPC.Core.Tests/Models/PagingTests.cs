using SPC.Core.Models;
using Xunit;

namespace SPC.Core.Tests.Models;

public class PagingTests
{
    [Fact]
    public void Slice_ReturnsRequestedPage()
    {
        var source = Enumerable.Range(1, 30).ToList();

        var page = Paging.Slice(source, page: 2, pageSize: 10);

        Assert.Equal(10, page.Items.Count);
        Assert.Equal(11, page.Items[0]);
        Assert.Equal(20, page.Items[^1]);
        Assert.Equal(2, page.Page);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(30, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.True(page.HasPrevious);
        Assert.True(page.HasNext);
    }

    [Fact]
    public void Slice_ClampsPagePastTheEnd()
    {
        var source = Enumerable.Range(1, 12).ToList();

        var page = Paging.Slice(source, page: 99, pageSize: 10);

        Assert.Equal(2, page.Page);
        Assert.Equal(2, page.Items.Count);
        Assert.False(page.HasNext);
    }

    [Fact]
    public void Slice_EmptySource_IsPageOne()
    {
        var page = Paging.Slice(Array.Empty<int>(), page: 3, pageSize: 25);

        Assert.Equal(1, page.Page);
        Assert.Equal(1, page.TotalPages);
        Assert.Equal(0, page.TotalCount);
        Assert.Empty(page.Items);
        Assert.False(page.HasPrevious);
        Assert.False(page.HasNext);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(25, 25)]
    [InlineData(50, 50)]
    [InlineData(7, 10)]
    [InlineData(100, 10)]
    public void NormalizePageSize_AllowsOnlySupportedSizes(int requested, int expected)
    {
        Assert.Equal(expected, Paging.NormalizePageSize(requested));
    }
}
