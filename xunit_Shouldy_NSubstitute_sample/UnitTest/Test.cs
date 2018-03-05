using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Xunit;

namespace UnitTest
{
    public class NonQueryTests
    {
        [Fact]
        public void CreateBlog_saves_a_blog_via_context()
        {
            var mockSet = Substitute.For<DbSet<Blog>>();
            var mockContext = Substitute.For<BloggingContext>();
            mockContext.Blogs.Returns(mockSet);

            var addBolg = new Blog(); // 用来验证add 方法 传入的参数使用为预期内容
            mockSet.Add(Arg.Do<Blog>(x => addBolg = x));

            var service = new BlogService(mockContext);
            service.AddBlog("ADO.NET Blog", "http://blogs.msdn.com/adonet");

            mockSet.Received(1).Add(Arg.Any<Blog>()); // set add 方法调用一次
            mockContext.Received(1).SaveChanges(); // context savechanges 方法调用一次

            // 实体属性验证
            addBolg.Name.ShouldBe("ADO.NET Blog");
            addBolg.Url.ShouldBe("http://blogs.msdn.com/adonet");

            addBolg.Name.ShouldNotBe("ADO.NET");
        }

        [Fact]
        public void GetAllBlogs_orders_by_name()
        {
            var data = new List<Blog>
            {
                new Blog { Name = "BBB" },
                new Blog { Name = "ZZZ" },
                new Blog { Name = "AAA" },
            }.AsQueryable();

            var mockSet = Substitute.For<DbSet<Blog>, IQueryable<Blog>>();
            ((IQueryable<Blog>)mockSet).Provider.Returns(data.Provider);
            ((IQueryable<Blog>)mockSet).Expression.Returns(data.Expression);
            ((IQueryable<Blog>)mockSet).ElementType.Returns(data.ElementType);
            ((IQueryable<Blog>)mockSet).GetEnumerator().Returns(data.GetEnumerator());

            var mockContext = Substitute.For<BloggingContext>();
            mockContext.Blogs.Returns(mockSet);
            var service = new BlogService(mockContext);
            var blogs = service.GetAllBlogs();
            blogs.Count.ShouldBe(3);
            blogs[0].Name.ShouldBe("AAA");
            blogs[1].Name.ShouldBe("BBB");
            blogs[2].Name.ShouldBe("ZZZ");

        }

        [Fact]
        public async Task GetAllBlogsAsync_orders_by_name()
        {
            var data = new List<Blog>
            {
                new Blog { Name = "BBB" },
                new Blog { Name = "ZZZ" },
                new Blog { Name = "AAA" },
            }.AsQueryable();

            var mockSet = Substitute.For<DbSet<Blog>, IQueryable<Blog>, IDbAsyncEnumerable<Blog>>();

            ((IDbAsyncEnumerable<Blog>)mockSet).GetAsyncEnumerator()
                .Returns(new TestDbAsyncEnumerator<Blog>(data.GetEnumerator()));
            ((IQueryable<Blog>)mockSet).Provider
                .Returns(new TestDbAsyncQueryProvider<Blog>(data.Provider));

            ((IQueryable<Blog>)mockSet).Expression
                .Returns(data.Expression);
            ((IQueryable<Blog>)mockSet).ElementType
                .Returns(data.ElementType);
            ((IQueryable<Blog>)mockSet).GetEnumerator()
                .Returns(data.GetEnumerator());

            var mockContext = Substitute.For<BloggingContext>();
            mockContext.Blogs.Returns(mockSet);
            var service = new BlogService(mockContext);
            var blogs = await service.GetAllBlogsAsync();
            blogs.Count.ShouldBe(3);
            blogs[0].Name.ShouldBe("AAA");
            blogs[1].Name.ShouldBe("BBB");
            blogs[2].Name.ShouldBe("ZZZ");
        }
    }
}
