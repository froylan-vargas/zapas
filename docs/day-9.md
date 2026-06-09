# Day 9 - Live Coding Intensive

## Instructor Goal

Today you are practicing how to solve live-coding problems while thinking out loud like a senior .NET engineer.

By the end of the day, you should be able to:

- Restate a problem clearly before coding
- Ask clarifying questions without stalling
- Choose an appropriate data structure
- Write clean C# under time pressure
- Test your own code manually
- Explain time and space complexity
- Discuss edge cases and production tradeoffs
- Recover calmly when you make a mistake

The objective is not to memorize solutions. The objective is to practice a repeatable interview process.

---

## 5-Hour Structure

| Activity | Time |
|---|---:|
| Problem 1 - Valid Parentheses | 45 min |
| Problem 2 - Merge Intervals | 45 min |
| Problem 3 - LRU Cache | 45 min |
| Problem 4 - Rate Limiter | 45 min |
| Problem 5 - In-memory repository | 45 min |
| Review, refactor, speaking practice | 75 min |

Main rule for today:

> Talk while you code. The interviewer is evaluating your reasoning, not only the final answer.

---

## Live Coding Template

Use this structure for every problem.

### Step 1 - Restate

Say:

> "Let me restate the problem to make sure I understand it."

Then describe the input, output, and main rule in your own words.

### Step 2 - Clarify

Ask only useful questions:

- Can the input be null or empty?
- Are values sorted?
- Are duplicates allowed?
- What should happen for invalid input?
- Do we need thread safety?
- What are the expected constraints?

### Step 3 - Approach

Say:

> "First I will solve the simple correct version. Then I will check edge cases and complexity."

Name the data structure:

- Stack
- Dictionary
- Queue
- Sorted list
- Linked list
- Hash set

### Step 4 - Code

Write readable code first. Avoid clever one-liners.

### Step 5 - Test Manually

Run through at least 3 cases:

- Happy path
- Empty or minimal input
- Edge case

### Step 6 - Complexity

Say:

> "Time complexity is O(...), and space complexity is O(...)."

### Step 7 - Production Notes

For backend-style problems, mention:

- Thread safety
- Validation
- Logging
- Persistence
- Expiration/cleanup
- Tests

---

# Problem 1 - Valid Parentheses

## Prompt

Given a string containing only the characters `(`, `)`, `{`, `}`, `[`, and `]`, determine if the input string is valid.

A string is valid when:

- Open brackets are closed by the same type of bracket.
- Open brackets are closed in the correct order.
- Every closing bracket has a matching opening bracket.

Examples:

```text
"()"       -> true
"()[]{}"   -> true
"(]"       -> false
"([)]"     -> false
"{[]}"     -> true
```

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need to scan the string and make sure every closing bracket matches the most recent unmatched opening bracket."

### Step 2 - Clarify

Ask:

- Can the string be empty?
- Will the input contain only bracket characters?
- Should `null` return false or throw?

For an interview, use this assumption:

```text
Empty string is valid. Null is invalid.
```

### Step 3 - Choose The Data Structure

Use a stack.

Why:

- The most recent opening bracket must be closed first.
- That is last-in, first-out behavior.

### Step 4 - Algorithm

```text
Create empty stack.

For each character:
  If opening bracket:
    Push it.
  If closing bracket:
    If stack is empty, return false.
    Pop the last opening bracket.
    If it does not match, return false.

At the end:
  Return true only if stack is empty.
```

### Step 5 - Solution

```csharp
public static bool IsValidParentheses(string? input)
{
    if (input is null)
    {
        return false;
    }

    var stack = new Stack<char>();

    foreach (var current in input)
    {
        if (current is '(' or '[' or '{')
        {
            stack.Push(current);
            continue;
        }

        if (stack.Count == 0)
        {
            return false;
        }

        var open = stack.Pop();

        if (!IsMatchingPair(open, current))
        {
            return false;
        }
    }

    return stack.Count == 0;
}

private static bool IsMatchingPair(char open, char close)
{
    return (open == '(' && close == ')')
        || (open == '[' && close == ']')
        || (open == '{' && close == '}');
}
```

### Step 6 - Manual Tests

```csharp
Console.WriteLine(IsValidParentheses("()"));       // true
Console.WriteLine(IsValidParentheses("()[]{}"));   // true
Console.WriteLine(IsValidParentheses("(]"));       // false
Console.WriteLine(IsValidParentheses("([)]"));     // false
Console.WriteLine(IsValidParentheses("{[]}"));     // true
Console.WriteLine(IsValidParentheses(""));         // true
Console.WriteLine(IsValidParentheses("("));        // false
Console.WriteLine(IsValidParentheses(")"));        // false
```

### Complexity

```text
Time: O(n), where n is the length of the string.
Space: O(n), in the worst case all characters are opening brackets.
```

### Interview Explanation

> "A stack is the natural fit because nested brackets must close in reverse order. I return false immediately for a closing bracket without a matching opener or for a mismatched pair. At the end, the stack must be empty."

---

# Problem 2 - Merge Intervals

## Prompt

Given a collection of intervals, merge all overlapping intervals.

Examples:

```text
[[1,3], [2,6], [8,10], [15,18]]
  -> [[1,6], [8,10], [15,18]]

[[1,4], [4,5]]
  -> [[1,5]]
```

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need to combine intervals when their ranges overlap or touch, and return the smallest list of non-overlapping intervals."

### Step 2 - Clarify

Ask:

- Are intervals already sorted?
- Should `[1,4]` and `[4,5]` be merged?
- Can start be greater than end?
- Should the original list be mutated?

For an interview, use these assumptions:

```text
Intervals are not guaranteed to be sorted.
Touching intervals should merge.
Invalid intervals are rejected.
The original input should not be mutated.
```

### Step 3 - Choose The Approach

Sort by start time, then scan.

Why:

- Once sorted, overlapping intervals appear next to each other.
- You only need to compare the current interval with the last merged interval.

### Step 4 - Define A Small Type

```csharp
public readonly record struct Interval(int Start, int End);
```

### Step 5 - Algorithm

```text
If input is empty, return empty list.
Validate each interval.
Sort intervals by Start, then End.
Create result list with the first interval.

For each next interval:
  Compare it with the last result interval.
  If next.Start <= last.End:
    Merge them.
  Else:
    Add next interval as a new range.

Return result.
```

### Step 6 - Solution

```csharp
public readonly record struct Interval(int Start, int End);

public static IReadOnlyList<Interval> MergeIntervals(IEnumerable<Interval> intervals)
{
    ArgumentNullException.ThrowIfNull(intervals);

    var sorted = intervals
        .Select(interval =>
        {
            if (interval.Start > interval.End)
            {
                throw new ArgumentException(
                    $"Invalid interval [{interval.Start}, {interval.End}].");
            }

            return interval;
        })
        .OrderBy(interval => interval.Start)
        .ThenBy(interval => interval.End)
        .ToList();

    if (sorted.Count == 0)
    {
        return [];
    }

    var merged = new List<Interval> { sorted[0] };

    foreach (var current in sorted.Skip(1))
    {
        var last = merged[^1];

        if (current.Start <= last.End)
        {
            merged[^1] = last with
            {
                End = Math.Max(last.End, current.End)
            };

            continue;
        }

        merged.Add(current);
    }

    return merged;
}
```

### Step 7 - Manual Tests

```csharp
Print(MergeIntervals([
    new Interval(1, 3),
    new Interval(2, 6),
    new Interval(8, 10),
    new Interval(15, 18)
])); // [1,6], [8,10], [15,18]

Print(MergeIntervals([
    new Interval(1, 4),
    new Interval(4, 5)
])); // [1,5]

Print(MergeIntervals([])); // empty

static void Print(IReadOnlyList<Interval> intervals)
{
    Console.WriteLine(string.Join(", ", intervals.Select(i => $"[{i.Start},{i.End}]")));
}
```

### Complexity

```text
Time: O(n log n), because sorting dominates.
Space: O(n), for the sorted copy and result list.
```

### Interview Explanation

> "Sorting makes the problem much simpler because any overlap must be adjacent in the sorted order. Then I scan once and merge with the last interval in the result."

---

# Problem 3 - LRU Cache

## Prompt

Design an LRU cache with fixed capacity.

It should support:

- `Get(key)`: return the value if present, otherwise return missing.
- `Put(key, value)`: add or update a value.
- When capacity is exceeded, remove the least recently used item.

Expected complexity:

```text
Get: O(1)
Put: O(1)
```

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need a cache that keeps recently used items and evicts the item that has gone unused the longest."

### Step 2 - Clarify

Ask:

- What happens if capacity is zero?
- Should updating an existing key count as usage?
- Do we need thread safety?
- Should missing values throw, return null, or return a boolean?

For an interview, use these assumptions:

```text
Capacity must be greater than zero.
Updating a key makes it most recently used.
Use TryGetValue instead of returning null.
No thread safety unless requested.
```

### Step 3 - Choose The Data Structures

Use:

- `Dictionary<TKey, LinkedListNode<Entry>>` for O(1) lookup
- `LinkedList<Entry>` for O(1) recency updates

Convention:

```text
Front = most recently used
Back = least recently used
```

### Step 4 - Algorithm

```text
Get:
  If key missing, return false.
  Move node to front.
  Return true and value.

Put:
  If key exists:
    Update value.
    Move node to front.
  Else:
    If at capacity:
      Remove last linked-list node and dictionary entry.
    Add new node to front.
    Add dictionary entry.
```

### Step 5 - Solution

```csharp
public sealed class LruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _nodes = new();
    private readonly LinkedList<CacheEntry> _usage = new();

    public LruCache(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Capacity must be greater than zero.");
        }

        _capacity = capacity;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (!_nodes.TryGetValue(key, out var node))
        {
            value = default!;
            return false;
        }

        MoveToFront(node);
        value = node.Value.Value;
        return true;
    }

    public void Put(TKey key, TValue value)
    {
        if (_nodes.TryGetValue(key, out var existingNode))
        {
            existingNode.Value = existingNode.Value with { Value = value };
            MoveToFront(existingNode);
            return;
        }

        if (_nodes.Count == _capacity)
        {
            RemoveLeastRecentlyUsed();
        }

        var entry = new CacheEntry(key, value);
        var node = new LinkedListNode<CacheEntry>(entry);

        _usage.AddFirst(node);
        _nodes[key] = node;
    }

    private void MoveToFront(LinkedListNode<CacheEntry> node)
    {
        if (node.List == _usage && node.Previous is null)
        {
            return;
        }

        _usage.Remove(node);
        _usage.AddFirst(node);
    }

    private void RemoveLeastRecentlyUsed()
    {
        var leastRecentlyUsed = _usage.Last;

        if (leastRecentlyUsed is null)
        {
            return;
        }

        _usage.RemoveLast();
        _nodes.Remove(leastRecentlyUsed.Value.Key);
    }

    private sealed record CacheEntry(TKey Key, TValue Value);
}
```

### Step 6 - Manual Tests

```csharp
var cache = new LruCache<int, string>(2);

cache.Put(1, "one");
cache.Put(2, "two");

Console.WriteLine(cache.TryGetValue(1, out var one)); // true
Console.WriteLine(one);                               // one

cache.Put(3, "three"); // Evicts key 2.

Console.WriteLine(cache.TryGetValue(2, out _));       // false
Console.WriteLine(cache.TryGetValue(1, out _));       // true
Console.WriteLine(cache.TryGetValue(3, out _));       // true
```

### Complexity

```text
Time:
  Get: O(1)
  Put: O(1)

Space:
  O(capacity)
```

### Production Notes

If this were production code, discuss:

- Thread safety with `lock`, `ReaderWriterLockSlim`, or a concurrent design
- Expiration by time, not only capacity
- Metrics for hit rate and eviction count
- Memory pressure
- Whether to use `IMemoryCache` instead of a custom implementation

### Interview Explanation

> "The dictionary gives constant-time lookup by key. The linked list gives constant-time movement to the front and removal from the back. Together they satisfy O(1) get and put."

---

# Problem 4 - Rate Limiter

## Prompt

Implement a simple in-memory rate limiter.

Example requirement:

```text
Allow at most 3 requests per user in a 10-second window.
```

The method should return:

```text
true  -> request is allowed
false -> request is rejected
```

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need to track recent request timestamps per key and reject requests once the key has reached the limit inside the configured time window."

### Step 2 - Clarify

Ask:

- Is the limit per user, IP address, API key, or endpoint?
- Is the window fixed or sliding?
- Should this be thread-safe?
- Should rejected requests count?
- Do we need cleanup of old keys?

For an interview, implement:

```text
Per-key sliding window.
Rejected requests do not count.
Thread-safe in-memory implementation.
```

### Step 3 - Choose The Data Structure

Use:

- `Dictionary<string, Queue<DateTimeOffset>>`
- One queue per rate-limit key
- A lock to protect shared state

Why a queue:

- Oldest request timestamp is at the front.
- Expired timestamps can be removed efficiently.

### Step 4 - Algorithm

```text
For a key and current time:
  Get or create its timestamp queue.
  Remove timestamps older than the window.
  If queue count is already at limit:
    Return false.
  Enqueue current timestamp.
  Return true.
```

### Step 5 - Solution

```csharp
public sealed class SlidingWindowRateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly Dictionary<string, Queue<DateTimeOffset>> _requestsByKey = new();
    private readonly object _gate = new();

    public SlidingWindowRateLimiter(int limit, TimeSpan window)
    {
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                "Limit must be greater than zero.");
        }

        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(window),
                "Window must be greater than zero.");
        }

        _limit = limit;
        _window = window;
    }

    public bool AllowRequest(string key, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key is required.", nameof(key));
        }

        lock (_gate)
        {
            if (!_requestsByKey.TryGetValue(key, out var timestamps))
            {
                timestamps = new Queue<DateTimeOffset>();
                _requestsByKey[key] = timestamps;
            }

            RemoveExpiredRequests(timestamps, now);

            if (timestamps.Count >= _limit)
            {
                return false;
            }

            timestamps.Enqueue(now);
            return true;
        }
    }

    private void RemoveExpiredRequests(
        Queue<DateTimeOffset> timestamps,
        DateTimeOffset now)
    {
        var cutoff = now - _window;

        while (timestamps.Count > 0 && timestamps.Peek() <= cutoff)
        {
            timestamps.Dequeue();
        }
    }
}
```

### Step 6 - Manual Tests

```csharp
var limiter = new SlidingWindowRateLimiter(3, TimeSpan.FromSeconds(10));
var start = DateTimeOffset.UtcNow;

Console.WriteLine(limiter.AllowRequest("user-1", start));                  // true
Console.WriteLine(limiter.AllowRequest("user-1", start.AddSeconds(1)));    // true
Console.WriteLine(limiter.AllowRequest("user-1", start.AddSeconds(2)));    // true
Console.WriteLine(limiter.AllowRequest("user-1", start.AddSeconds(3)));    // false
Console.WriteLine(limiter.AllowRequest("user-1", start.AddSeconds(11)));   // true

Console.WriteLine(limiter.AllowRequest("user-2", start.AddSeconds(3)));    // true
```

### Complexity

```text
Time:
  Usually O(1) per request.
  Worst case O(k), where k is the number of expired timestamps removed for that key.

Space:
  O(number of active keys * limit)
```

### Production Notes

For Zapas, rate limiting protects expensive endpoints such as `POST /sessions`.

Production considerations:

- Use ASP.NET Core built-in rate limiting when possible.
- Use distributed storage such as Redis if multiple API instances run behind a load balancer.
- Consider limits per user, IP address, endpoint, or token.
- Add cleanup for inactive keys.
- Return HTTP `429 Too Many Requests`.
- Include retry metadata when useful.

### Interview Explanation

> "This is a sliding-window limiter. I keep recent timestamps per key, drop expired entries, and allow the request only if the remaining count is below the limit. For a single-instance API this works in memory, but for multiple instances I would use a distributed store or built-in middleware backed by shared infrastructure."

---

# Problem 5 - In-Memory Repository

## Prompt

Implement a simple in-memory repository for sessions.

It should support:

- Add a session
- Get one session by id
- List sessions with pagination
- Delete a session

This mirrors the Zapas repository conversation and helps you explain why repositories are useful as boundaries.

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need a small persistence abstraction backed by memory, with async method signatures so it can later be replaced by EF Core without changing controllers or services."

### Step 2 - Clarify

Ask:

- Should ids be generated inside the repository or provided by the caller?
- Should list results be sorted?
- Should paging be one-based or zero-based?
- Should this be thread-safe?
- Should returned objects be mutable references or copies?

For this exercise, use these assumptions:

```text
Session ids are provided by the domain model.
Pagination is one-based.
Results are sorted by StartTime descending.
The repository is thread-safe.
The method signatures are async-compatible.
```

### Step 3 - Define Minimal Models

```csharp
public sealed record Session(
    Guid Id,
    string Name,
    DateTimeOffset StartTime,
    double DistanceMeters);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
```

### Step 4 - Define The Interface

```csharp
public interface ISessionRepository
{
    Task<Session> AddAsync(Session session, CancellationToken cancellationToken);

    Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<Session>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
```

### Step 5 - Implementation

```csharp
public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly Dictionary<Guid, Session> _sessions = new();
    private readonly object _gate = new();

    public Task<Session> AddAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        if (session.Id == Guid.Empty)
        {
            throw new ArgumentException("Session id is required.", nameof(session));
        }

        lock (_gate)
        {
            if (_sessions.ContainsKey(session.Id))
            {
                throw new InvalidOperationException(
                    $"Session '{session.Id}' already exists.");
            }

            _sessions.Add(session.Id, session);
        }

        return Task.FromResult(session);
    }

    public Task<Session?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            _sessions.TryGetValue(id, out var session);
            return Task.FromResult(session);
        }
    }

    public Task<PagedResult<Session>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (page <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be greater than zero.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be greater than zero.");
        }

        lock (_gate)
        {
            var totalCount = _sessions.Count;

            var items = _sessions.Values
                .OrderByDescending(session => session.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PagedResult<Session>(
                items,
                page,
                pageSize,
                totalCount);

            return Task.FromResult(result);
        }
    }

    public Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            return Task.FromResult(_sessions.Remove(id));
        }
    }
}
```

### Step 6 - Manual Tests

```csharp
var repository = new InMemorySessionRepository();
var cancellationToken = CancellationToken.None;

var first = new Session(
    Guid.NewGuid(),
    "Morning run",
    DateTimeOffset.UtcNow.AddDays(-1),
    5000);

var second = new Session(
    Guid.NewGuid(),
    "Tempo run",
    DateTimeOffset.UtcNow,
    8000);

await repository.AddAsync(first, cancellationToken);
await repository.AddAsync(second, cancellationToken);

var loaded = await repository.GetByIdAsync(first.Id, cancellationToken);
Console.WriteLine(loaded?.Name); // Morning run

var page = await repository.ListAsync(1, 10, cancellationToken);
Console.WriteLine(page.TotalCount);       // 2
Console.WriteLine(page.Items[0].Name);    // Tempo run

var deleted = await repository.DeleteAsync(first.Id, cancellationToken);
Console.WriteLine(deleted);               // true

var missing = await repository.GetByIdAsync(first.Id, cancellationToken);
Console.WriteLine(missing is null);       // true
```

### Complexity

```text
Add: O(1)
GetById: O(1)
Delete: O(1)
List: O(n log n), because this implementation sorts on each list call.
Space: O(n)
```

### Production Notes

For Zapas:

- In-memory storage is useful for early development and tests.
- It is not durable.
- It does not work across multiple API instances.
- It loses data on restart.
- EF Core is a better production persistence option.
- The repository boundary lets controllers and services avoid depending directly on storage details.

### Interview Explanation

> "I keep the repository async-compatible because the production implementation will likely use EF Core async database calls. The in-memory version returns completed tasks, which keeps the interface stable while allowing simple tests."

---

# Hour 5 - Review And Speaking Practice

## Review Checklist

For each problem, make sure you can answer:

- What data structure did I use?
- Why did I choose it?
- What are the edge cases?
- What is the time complexity?
- What is the space complexity?
- What would change in production?

---

## Common Mistakes To Avoid

### Mistake 1 - Coding Silently

Bad:

```text
Long silence while typing.
```

Better:

> "I am using a dictionary here because I need constant-time lookup. The linked list will keep track of recency."

### Mistake 2 - Starting With Code Immediately

Bad:

```text
Opens with code before confirming requirements.
```

Better:

> "Before coding, I want to confirm whether the input is sorted and whether touching intervals should merge."

### Mistake 3 - Ignoring Invalid Input

Bad:

```text
Assumes all inputs are valid.
```

Better:

> "I will validate capacity because a zero-capacity LRU cache does not make sense under this API."

### Mistake 4 - Not Testing

Bad:

```text
Finishes code and says done.
```

Better:

> "Let me walk through a normal case, an empty case, and a boundary case."

### Mistake 5 - Overengineering

Bad:

```text
Adds many abstractions before solving the problem.
```

Better:

> "For the interview version, I will keep the implementation small. Then I can discuss thread safety, metrics, and persistence as production concerns."

---

# Senior-Level Follow-Up Questions

## Valid Parentheses

Possible follow-ups:

- What if the input contains non-bracket characters?
- What if the input is a stream?
- Could this be done without extra memory?

Strong answer:

> "For arbitrary nested brackets, a stack is still needed because I must remember unmatched openers. If non-bracket characters are allowed, I would either ignore them or reject them depending on the requirement."

---

## Merge Intervals

Possible follow-ups:

- What if intervals arrive as a stream?
- What if there are millions of intervals?
- What if intervals are already sorted?

Strong answer:

> "If intervals are already sorted, the algorithm becomes O(n). For very large inputs, I would avoid unnecessary copies and consider streaming or database-side sorting if the data already lives in storage."

---

## LRU Cache

Possible follow-ups:

- Make it thread-safe.
- Add expiration.
- Add cache metrics.
- Compare with `IMemoryCache`.

Strong answer:

> "In production .NET code I would usually start with `IMemoryCache` because it already handles expiration, eviction, and framework integration. I would implement a custom cache only if the requirements were specific enough to justify it."

---

## Rate Limiter

Possible follow-ups:

- How would this work with multiple API instances?
- What status code should an API return?
- How do you avoid unbounded memory growth?

Strong answer:

> "For multiple API instances, in-memory counters are not enough because each instance has a partial view. I would use ASP.NET Core rate limiting with a shared backing store or put the limit at the gateway/load balancer layer."

---

## In-Memory Repository

Possible follow-ups:

- Replace it with EF Core.
- Add filtering and sorting.
- Add optimistic concurrency.
- Add owner-based authorization.

Strong answer:

> "The repository interface lets the service depend on a storage capability rather than a storage technology. The EF Core version would use `AsNoTracking` for read queries, pagination in the database, and cancellation tokens for async database calls."

---

# Zapas Interview Tie-In

Use these connections when an interviewer asks how live-coding relates to real backend work.

## Stack

Valid Parentheses shows:

- Basic data structure fluency
- Input validation
- Early exits

Zapas connection:

```text
Parsing file-like or nested input often requires validating structure before trusting the payload.
```

## Sorting And Scanning

Merge Intervals shows:

- Sorting
- Linear scanning
- Range merging

Zapas connection:

```text
Run intervals, activity windows, date filters, and time ranges often use sorting and overlap logic.
```

## Cache Design

LRU Cache shows:

- Dictionary lookup
- Linked list updates
- Eviction policy

Zapas connection:

```text
GET /sessions/{id} can benefit from caching, but production cache behavior needs invalidation, expiration, and metrics.
```

## Rate Limiting

Rate Limiter shows:

- Queue usage
- Per-user tracking
- Guardrails for expensive endpoints

Zapas connection:

```text
POST /sessions is expensive because it accepts files and runs FIT parsing, so rate limiting protects API availability.
```

## Repository

In-memory repository shows:

- Interface design
- Persistence boundary
- Pagination
- Cancellation-friendly signatures

Zapas connection:

```text
Zapas can start with an in-memory repository for practice, then move to EF Core without changing controller behavior.
```

---

# Final Day 9 Deliverable

At the end of today, create or update your notes with:

```markdown
## Day 9 Live Coding Review

### Problems I Can Explain Well

- Valid Parentheses
- Merge Intervals
- LRU Cache
- Rate Limiter
- In-memory Repository

### Patterns Practiced

- Stack
- Dictionary
- Queue
- Sorting and scanning
- Linked list
- Repository abstraction
- Pagination
- Thread-safety discussion

### Phrases I Will Use In Interviews

- "Let me restate the problem first."
- "I will start with the simple correct version."
- "The data structure choice here is important because..."
- "Let me test a normal case, an edge case, and a failure case."
- "The production version would need..."

### Mistakes To Watch

- Coding silently
- Forgetting edge cases
- Skipping complexity analysis
- Overengineering the first version
- Not connecting the solution back to backend work
```

---

# Final Interview Phrasing

Use this at the end of a live-coding problem:

> "This solution is intentionally straightforward. It handles the main edge cases, has predictable complexity, and keeps the code readable. If this were production code, I would add focused unit tests and then consider thread safety, logging, metrics, and persistence depending on where the code runs."
