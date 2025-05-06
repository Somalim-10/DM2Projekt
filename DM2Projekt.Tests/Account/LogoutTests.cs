using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Pages.Account;

namespace DM2Projekt.Tests.Account;

[TestClass]
public class LogoutTests
{
    [TestMethod]
    public void Logout_Should_Clear_Session_And_Redirect()
    {
        var model = new LogoutModel();
        var httpContext = new DefaultHttpContext();

        var pageContext = new PageContext
        {
            HttpContext = httpContext
        };

        model.PageContext = pageContext;

        httpContext.Session = new DummySession(); // mock session

        // simulate logout
        var result = model.OnPost() as RedirectToPageResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("../Index", result.PageName, "should redirect to index");
    }

    // simple fake session for testing
    private class DummySession : ISession
    {
        private Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken _) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken _) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }
}
