using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            // --- Interests ---
            var interests = new List<Interest>
            {
                new() { Name = "Machine Learning", Category = "Technology" },
                new() { Name = "Web Development", Category = "Technology" },
                new() { Name = "Data Science", Category = "Technology" },
                new() { Name = "Cloud Computing", Category = "Technology" },
                new() { Name = "Cybersecurity", Category = "Technology" },
                new() { Name = "Mobile Development", Category = "Technology" },
                new() { Name = "Photography", Category = "Creative" },
                new() { Name = "Gaming", Category = "Entertainment" },
                new() { Name = "Music", Category = "Creative" },
                new() { Name = "Fitness", Category = "Health" },
                new() { Name = "Reading", Category = "Education" },
                new() { Name = "Travel", Category = "Lifestyle" },
                new() { Name = "Entrepreneurship", Category = "Business" },
                new() { Name = "UI/UX Design", Category = "Creative" },
                new() { Name = "DevOps", Category = "Technology" },
            };
            context.Interests.AddRange(interests);

            // --- Skills ---
            var skills = new List<Skill>
            {
                new() { Name = "C#", Category = "Programming" },
                new() { Name = "Python", Category = "Programming" },
                new() { Name = "JavaScript", Category = "Programming" },
                new() { Name = "React", Category = "Frontend" },
                new() { Name = "ASP.NET Core", Category = "Backend" },
                new() { Name = "SQL Server", Category = "Database" },
                new() { Name = "Azure", Category = "Cloud" },
                new() { Name = "Docker", Category = "DevOps" },
                new() { Name = "Machine Learning", Category = "AI" },
                new() { Name = "TypeScript", Category = "Programming" },
                new() { Name = "Figma", Category = "Design" },
                new() { Name = "Node.js", Category = "Backend" },
                new() { Name = "Kubernetes", Category = "DevOps" },
                new() { Name = "TensorFlow", Category = "AI" },
                new() { Name = "GraphQL", Category = "API" },
            };
            context.Skills.AddRange(skills);
            await context.SaveChangesAsync();

            // --- 10 Sample Users ---
            var users = new List<User>
            {
                new()
                {
                    Username = "alex_dev",
                    Email = "alex@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Alex Johnson",
                    Bio = "Full-stack developer passionate about building scalable systems. Love C# and React!",
                    Location = "San Francisco, CA",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=alex",
                    CreatedAt = DateTime.UtcNow.AddDays(-120)
                },
                new()
                {
                    Username = "sarah_ml",
                    Email = "sarah@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Sarah Chen",
                    Bio = "ML Engineer at a startup. Researching NLP and computer vision. Python enthusiast.",
                    Location = "Seattle, WA",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=sarah",
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                },
                new()
                {
                    Username = "mike_cloud",
                    Email = "mike@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Michael Rodriguez",
                    Bio = "Cloud architect specializing in Azure and AWS. DevOps advocate. Kubernetes lover.",
                    Location = "Austin, TX",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=mike",
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                },
                new()
                {
                    Username = "priya_design",
                    Email = "priya@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Priya Patel",
                    Bio = "UI/UX designer who codes. Creating beautiful, accessible experiences with Figma and React.",
                    Location = "New York, NY",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=priya",
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new()
                {
                    Username = "james_sec",
                    Email = "james@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "James Williams",
                    Bio = "Cybersecurity professional. Ethical hacker, CTF enthusiast, and security researcher.",
                    Location = "Washington, DC",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=james",
                    CreatedAt = DateTime.UtcNow.AddDays(-75)
                },
                new()
                {
                    Username = "emily_data",
                    Email = "emily@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Emily Thompson",
                    Bio = "Data scientist turning raw numbers into business insights. SQL and Python are my superpowers.",
                    Location = "Chicago, IL",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=emily",
                    CreatedAt = DateTime.UtcNow.AddDays(-65)
                },
                new()
                {
                    Username = "raj_mobile",
                    Email = "raj@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Raj Sharma",
                    Bio = "Mobile developer building cross-platform apps with React Native. Startup founder.",
                    Location = "Boston, MA",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=raj",
                    CreatedAt = DateTime.UtcNow.AddDays(-55)
                },
                new()
                {
                    Username = "lisa_devops",
                    Email = "lisa@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Lisa Park",
                    Bio = "DevOps engineer automating everything. CI/CD pipelines, Docker, Kubernetes, and more.",
                    Location = "Portland, OR",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=lisa",
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },
                new()
                {
                    Username = "carlos_backend",
                    Email = "carlos@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Carlos Mendez",
                    Bio = "Backend engineer specializing in microservices with ASP.NET Core and SQL Server.",
                    Location = "Miami, FL",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=carlos",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    Username = "zoe_ai",
                    Email = "zoe@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    FullName = "Zoe Anderson",
                    Bio = "AI researcher working on LLMs and generative models. PhD student in Computer Science.",
                    Location = "Boston, MA",
                    ProfilePictureUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=zoe",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // --- Assign Interests to Users ---
            var interestMap = interests.ToDictionary(i => i.Name, i => i.Id);
            var skillMap = skills.ToDictionary(s => s.Name, s => s.Id);

            var userInterestAssignments = new (string Username, string[] Interests)[]
            {
                ("alex_dev",    new[]{ "Web Development","Machine Learning","Entrepreneurship","Gaming" }),
                ("sarah_ml",    new[]{ "Machine Learning","Data Science","Reading","Fitness" }),
                ("mike_cloud",  new[]{ "Cloud Computing","DevOps","Travel","Gaming" }),
                ("priya_design",new[]{ "UI/UX Design","Photography","Music","Web Development" }),
                ("james_sec",   new[]{ "Cybersecurity","Gaming","Reading","Entrepreneurship" }),
                ("emily_data",  new[]{ "Data Science","Machine Learning","Reading","Fitness" }),
                ("raj_mobile",  new[]{ "Mobile Development","Entrepreneurship","Travel","Music" }),
                ("lisa_devops", new[]{ "DevOps","Cloud Computing","Fitness","Photography" }),
                ("carlos_backend",new[]{"Web Development","Cloud Computing","Gaming","Travel" }),
                ("zoe_ai",      new[]{ "Machine Learning","Data Science","Reading","Music" }),
            };

            var userSkillAssignments = new (string Username, string[] Skills)[]
            {
                ("alex_dev",     new[]{ "C#","React","ASP.NET Core","SQL Server","TypeScript" }),
                ("sarah_ml",     new[]{ "Python","Machine Learning","TensorFlow","SQL Server" }),
                ("mike_cloud",   new[]{ "Azure","Docker","Kubernetes","Python","C#" }),
                ("priya_design", new[]{ "Figma","React","JavaScript","TypeScript","CSS" }),
                ("james_sec",    new[]{ "Python","C#","SQL Server","Docker" }),
                ("emily_data",   new[]{ "Python","SQL Server","Machine Learning","TensorFlow" }),
                ("raj_mobile",   new[]{ "React","JavaScript","TypeScript","Node.js" }),
                ("lisa_devops",  new[]{ "Docker","Kubernetes","Azure","Python","C#" }),
                ("carlos_backend",new[]{ "C#","ASP.NET Core","SQL Server","Azure","Docker" }),
                ("zoe_ai",       new[]{ "Python","TensorFlow","Machine Learning","GraphQL" }),
            };

            var userDict = users.ToDictionary(u => u.Username, u => u.Id);

            foreach (var (username, userInterests) in userInterestAssignments)
            {
                foreach (var interest in userInterests)
                {
                    if (interestMap.TryGetValue(interest, out var interestId))
                    {
                        context.UserInterests.Add(new UserInterest
                        {
                            UserId = userDict[username],
                            InterestId = interestId
                        });
                    }
                }
            }

            foreach (var (username, userSkills) in userSkillAssignments)
            {
                foreach (var skill in userSkills)
                {
                    if (skillMap.TryGetValue(skill, out var skillId))
                    {
                        context.UserSkills.Add(new UserSkill
                        {
                            UserId = userDict[username],
                            SkillId = skillId,
                            Level = "Intermediate"
                        });
                    }
                }
            }

            await context.SaveChangesAsync();

            // --- Seed Connections ---
            var connectionPairs = new (string Sender, string Receiver)[]
            {
                ("alex_dev", "sarah_ml"),
                ("alex_dev", "carlos_backend"),
                ("alex_dev", "priya_design"),
                ("sarah_ml", "emily_data"),
                ("sarah_ml", "zoe_ai"),
                ("mike_cloud", "lisa_devops"),
                ("mike_cloud", "carlos_backend"),
                ("james_sec", "carlos_backend"),
                ("raj_mobile", "priya_design"),
                ("raj_mobile", "alex_dev"),
                ("emily_data", "zoe_ai"),
                ("lisa_devops", "zoe_ai"),
            };

            foreach (var (sender, receiver) in connectionPairs)
            {
                context.Connections.Add(new Connection
                {
                    SenderId = userDict[sender],
                    ReceiverId = userDict[receiver],
                    Status = ConnectionStatus.Accepted,
                    CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(5, 60))
                });
            }
            await context.SaveChangesAsync();

            // --- Seed Posts ---
            var rand = new Random(42);
            var posts = new List<(string Username, string Content, string[] Tags)>
            {
                ("alex_dev", "Just finished building a graph-based recommendation engine in C#! The BFS traversal for second-degree connections improved suggestion relevance by ~35%. Really proud of this one. #csharp #algorithms #backend", new[]{"csharp","algorithms","backend"}),
                ("alex_dev", "ASP.NET Core + React is such a powerful combo for full-stack development. Clean separation of concerns, great performance, and wonderful dev experience. What's your favorite stack? #aspnet #react #webdev", new[]{"aspnet","react","webdev"}),
                ("alex_dev", "Implementing Trie data structures for search autocomplete reduced lookup time to O(L) — that's the length of the search term! Game changer for user experience. #datastructures #csharp", new[]{"datastructures","csharp"}),
                ("sarah_ml", "Trained a new NLP model today — 94.2% accuracy on sentiment analysis! The trick was using transfer learning on a domain-specific corpus. Sharing the paper link soon. #machinelearning #nlp #python", new[]{"machinelearning","nlp","python"}),
                ("sarah_ml", "Python's ecosystem for ML is unmatched. PyTorch, TensorFlow, scikit-learn — it's like having a full workshop in your hands. Which framework do you prefer? #python #datascience #ai", new[]{"python","datascience","ai"}),
                ("sarah_ml", "Data cleaning takes 80% of the time but gets 0% of the credit. Today's battle: 50,000 rows of missing values and inconsistent date formats. Coffee is essential. #datascience #python", new[]{"datascience","python"}),
                ("mike_cloud", "Migrated a monolith to microservices on Azure Kubernetes Service today. Deployment time dropped from 45 minutes to 8 minutes! Infrastructure as code really pays off. #azure #kubernetes #devops", new[]{"azure","kubernetes","devops"}),
                ("mike_cloud", "Docker tip: Always use multi-stage builds. Reduced our container image size from 1.2GB to 180MB. Faster deployments, lower costs, happier team. #docker #devops #cloudcomputing", new[]{"docker","devops","cloudcomputing"}),
                ("priya_design", "Accessibility isn't an afterthought — it's a core feature. Spent today making our React components WCAG 2.1 compliant. Every user deserves a great experience! #ux #accessibility #react", new[]{"ux","accessibility","react"}),
                ("priya_design", "Design systems are multipliers. We launched our component library built with Figma + React + TypeScript and team velocity has doubled. #design #react #typescript #figma", new[]{"design","react","typescript","figma"}),
                ("james_sec", "Completed another CTF challenge today — exploited a SQL injection vulnerability and gained admin access in under 10 minutes. Patching your inputs is NOT optional! #cybersecurity #ethicalhacking", new[]{"cybersecurity","ethicalhacking"}),
                ("james_sec", "Zero-trust architecture is the future of enterprise security. Never assume, always verify. Especially important for cloud-native applications. #security #zerotrust #cloud", new[]{"security","zerotrust","cloud"}),
                ("emily_data", "Built a real-time analytics dashboard using SQL Server temporal tables + React. Business stakeholders can now see live KPIs without bugging the data team! #datascience #sqlserver #react", new[]{"datascience","sqlserver","react"}),
                ("emily_data", "A heap-based priority queue in SQL? Yes, it's possible and surprisingly efficient for top-K queries. Sometimes the best tool is the one you already have. #sql #algorithms #dataengineering", new[]{"sql","algorithms","dataengineering"}),
                ("raj_mobile", "Shipped our React Native app to both App Store and Play Store simultaneously! Cross-platform development has come so far. One codebase, two platforms. #reactnative #mobile #startup", new[]{"reactnative","mobile","startup"}),
                ("raj_mobile", "Entrepreneurship lesson #47: Ship fast, learn faster. We've iterated on our product 12 times in 3 months based on user feedback. Progress over perfection! #startup #entrepreneurship", new[]{"startup","entrepreneurship"}),
                ("lisa_devops", "CI/CD pipeline completed in 4 minutes flat — build, test, deploy. Automated everything with GitHub Actions + Azure DevOps. No more manual deployments! #devops #cicd #azure", new[]{"devops","cicd","azure"}),
                ("lisa_devops", "Kubernetes tip: Use resource limits on ALL pods. We had a memory leak crash our production cluster last week. Lesson learned the expensive way. #kubernetes #devops #sre", new[]{"kubernetes","devops","sre"}),
                ("carlos_backend", "Optimized our most critical SQL Server query from 8 seconds to 120ms by adding proper indexing and rewriting the query plan. Sometimes fundamentals win. #sqlserver #performance #csharp", new[]{"sqlserver","performance","csharp"}),
                ("carlos_backend", "ASP.NET Core Minimal APIs are incredibly elegant for microservices. Clean code, fast performance, perfect for building scalable backends. #aspnet #dotnet #backend #microservices", new[]{"aspnet","dotnet","backend","microservices"}),
                ("zoe_ai", "Paper published! 'Efficient Attention Mechanisms for Long-Context LLMs' — reduced compute cost by 40% while maintaining accuracy. Will share the code on GitHub soon! #ai #llm #research", new[]{"ai","llm","research"}),
                ("zoe_ai", "The boundary between ML research and engineering is blurring. You now need to understand both model architecture AND systems design to ship truly great AI products. #ai #machinelearning #python", new[]{"ai","machinelearning","python"}),
            };

            foreach (var (username, content, tags) in posts)
            {
                var post = new Post
                {
                    UserId = userDict[username],
                    Content = content,
                    ViewCount = rand.Next(50, 2000),
                    TrendingScore = rand.NextDouble() * 100,
                    CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 90)),
                };
                foreach (var tag in tags)
                {
                    post.PostTags.Add(new PostTag { Tag = tag });
                }
                context.Posts.Add(post);
            }

            await context.SaveChangesAsync();
        }
    }
}
