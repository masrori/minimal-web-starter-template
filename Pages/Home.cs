using Orchestrate.Core;
using Orchestrate.Data;
using System.Text;

namespace Orchestrate.pages
{
    public static class Home
    {
        public static void MapHomeEndPoints(this WebApplication app)
        {
            app.MapGet("/", Index);
            app.MapGet("/groq", AskGroq);
        }
        public static async Task<IResult> AskGroq(HttpContext ctx, GroqClient groqClient)
        {
            var message = ctx.Request.Query["q"].ToString();

            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>Groq Chat</title>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("<h2>Chat AI (Groq)</h2>");

            // form tetap ada biar bisa kirim lagi
            sb.AppendLine("<form action='/groq' method='get'>");
            sb.AppendLine($"<input type='text' name='q' value='{message}' style='width:300px' />");
            sb.AppendLine("<button type='submit'>Kirim</button>");
            sb.AppendLine("</form>");

            if (!string.IsNullOrWhiteSpace(message))
            {
                var result = await groqClient.AskGroqAsync(message);

                sb.AppendLine("<hr/>");
                sb.AppendLine("<h4>Pertanyaan:</h4>");
                sb.AppendLine($"<pre>{System.Net.WebUtility.HtmlEncode(message)}</pre>");

                sb.AppendLine("<h4>Jawaban:</h4>");
                sb.AppendLine($"<pre>{System.Net.WebUtility.HtmlEncode(result)}</pre>");
            }

            sb.AppendLine("<br/><a href='/'>Kembali</a>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return Results.Content(sb.ToString(), "text/html", Encoding.UTF8);
        }
        //Login
        public static IResult Index(HttpContext context)
        {
            var html = """
                <!DOCTYPE html>
                <html lang="en"><head>
                <meta charset="utf-8"/>
                <meta content="width=device-width, initial-scale=1.0" name="viewport"/>
                <title>Indosfot P2P Lending</title>
                <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&amp;display=swap" rel="stylesheet"/>
                <link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&amp;display=swap" rel="stylesheet"/>
                <link href="/styles/oams.css" rel="stylesheet"/>
                </head>
                <body>
                <!-- Sidebar -->
                <aside class="sidebar">
                <div class="sidebar-logo">
                <div class="logo-icon">
                <span class="material-symbols-outlined" style="color: white">token</span>
                </div>
                <span class="logo-text">Online Loan</span>
                </div>
                <div class="sidebar-user">
                <img alt="Admin profile" class="user-avatar-small" src="https://lh3.googleusercontent.com/aida-public/AB6AXuAxWaAiXV4QexEOTMzHzDiW_GotzX3O2qu7gM94BcE4J1Zoo8yDNG7vhMSJYC0RRkiSAfsid8Ekvxcs1QCEReH66EmyO9IPTLbjQ4QFC91ZV6CloaHu1v04tyxjVIru8woZZ0OWVr27_DlYzsFx5P78awZ3gl5g_zJFd0Msf5z2U_6EQHx4MTOJkevabPaC6S9nQ4hcNU0h4zXS4PWrz3-RwLNakBrNTDF_JS07z5FhRgwZxfNYAGzL6APvKt1kfHwBRL0XjMnHCw"/>
                <div class="user-info">
                <p class="user-info-name">System Admin</p>
                <p class="user-info-status">online</p>
                </div>
                </div>
                <nav class="nav-list">
                <div class="nav-item">
                <a class="nav-link active" href="#"><span class="material-symbols-outlined">dashboard</span>Dashboard</a>
                </div>
                <div class="nav-item">
                <a class="nav-link" href="#"><span class="material-symbols-outlined">analytics</span>Analytics</a>
                </div>
                <div class="nav-item">
                <a class="nav-link" href="#"><span class="material-symbols-outlined">group</span>Team</a>
                </div>
                <div class="nav-item">
                <a class="nav-link" href="#"><span class="material-symbols-outlined">folder</span>Projects</a>
                </div>
                <div class="nav-item">
                <a class="nav-link" href="#"><span class="material-symbols-outlined">description</span>Reports</a>
                </div>
                <button class="new-entry-btn">
                <span class="material-symbols-outlined">add</span>New Entry
                            </button>
                </nav>
                <div class="sidebar-bottom">
                <a class="nav-link" href="#"><span class="material-symbols-outlined">help</span>Help</a>
                <a class="nav-link" href="#"><span class="material-symbols-outlined">logout</span>Logout</a>
                </div>
                </aside>
                <div class="main-wrapper">
                <!-- Top Nav -->
                <header>
                <div class="header-left">
                <span class="logo-text" style="color: var(--primary); text-transform: none;">Executive Curator</span>
                <div class="search-bar">
                <span class="material-symbols-outlined" style="color: var(--text-muted); font-size: 1.2rem;">search</span>
                <input placeholder="System search..." type="text"/>
                </div>
                </div>
                <div class="header-right">
                <span class="material-symbols-outlined" style="color: var(--text-muted); cursor: pointer;">notifications</span>
                <span class="material-symbols-outlined" style="color: var(--text-muted); cursor: pointer;">settings</span>
                <div class="user-profile-toggle">
                <img alt="User" src="https://lh3.googleusercontent.com/aida-public/AB6AXuD9-J7eXUlfSzpFAZHpbgGbxX-sndVP3UJODeZ26P7U4EARhS66qRYIHRFkXsa0cV5-ML98RezIZE-tE-SD0AjJJMQzeMAXEnJOJU7lPfAkKo-pNuSkDgg-d8y_tPSZ-EUmkRkXMuUy6cAr_DIzOhLmNxkPFSY8APw1ueXJBkLCoSp0LikqtROXnZdpluIpKgnzr41dpyRlb4NjGefAframMCqLt_6ROy-PC8nQEShcIgavGdrt8hP-U9hCGIUnAAGjV7-6qejmxw"/>
                <span style="font-size: 0.875rem; font-weight: 600;">Marcus A.</span>
                <span class="material-symbols-outlined" style="font-size: 1.2rem; color: var(--text-light);">expand_more</span>
                </div>
                </div>
                </header>
                <main>
                <!-- Page Heading -->
                <div class="page-header">
                <div>
                <nav class="breadcrumb">
                <span>Main</span> / <span class="active">Dashboard</span>
                </nav>
                <h1 class="page-title">Dashboard Overview</h1>
                </div>
                <div class="date-picker">
                <span class="material-symbols-outlined">calendar_today</span>
                                    Oct 24, 2023 - Oct 31, 2023
                                </div>
                </div>
                <!-- Stats Grid -->
                <div class="stats-grid">
                <div class="stat-card">
                <div class="stat-top">
                <div class="stat-icon" style="background-color: rgba(0, 123, 255, 0.1);">
                <span class="material-symbols-outlined" style="color: var(--primary); font-variation-settings: 'FILL' 1;">person</span>
                </div>
                <span class="stat-trend" style="background-color: rgba(0, 123, 255, 0.05); color: var(--primary);">+12%</span>
                </div>
                <div>
                <p class="stat-label">Total Users</p>
                <h3 class="stat-value">15,842</h3>
                </div>
                </div>
                <div class="stat-card">
                <div class="stat-top">
                <div class="stat-icon" style="background-color: #ffefea;">
                <span class="material-symbols-outlined" style="color: #9e3d00; font-variation-settings: 'FILL' 1;">payments</span>
                </div>
                <span class="stat-trend" style="background-color: #fff1eb; color: #9e3d00;">+5.2%</span>
                </div>
                <div>
                <p class="stat-label">Monthly Revenue</p>
                <h3 class="stat-value">$42,910</h3>
                </div>
                </div>
                <div class="stat-card">
                <div class="stat-top">
                <div class="stat-icon" style="background-color: #e0f2fe;">
                <span class="material-symbols-outlined" style="color: #0ea5e9; font-variation-settings: 'FILL' 1;">timer</span>
                </div>
                <span class="stat-trend" style="background-color: #f0f9ff; color: #0ea5e9;">-2%</span>
                </div>
                <div>
                <p class="stat-label">Active Sessions</p>
                <h3 class="stat-value">3,241</h3>
                </div>
                </div>
                <div class="stat-card">
                <div class="stat-top">
                <div class="stat-icon" style="background-color: #fee2e2;">
                <span class="material-symbols-outlined" style="color: #ef4444; font-variation-settings: 'FILL' 1;">shopping_cart</span>
                </div>
                <span class="stat-trend" style="background-color: #fef2f2; color: #ef4444;">+24%</span>
                </div>
                <div>
                <p class="stat-label">New Orders</p>
                <h3 class="stat-value">648</h3>
                </div>
                </div>
                </div>
                <!-- Middle Section -->
                <div class="insights-grid">
                <!-- Sales Analytics -->
                <div class="card-container">
                <div class="card-header">
                <div>
                <h2 class="card-title">Sales Analytics</h2>
                <p style="font-size: 0.875rem; color: var(--text-muted);">Performance over the last 30 days</p>
                </div>
                <div style="background: var(--surface-container); padding: 0.25rem; border-radius: var(--radius-md); display: flex;">
                <button style="border: none; background: white; padding: 0.25rem 0.75rem; border-radius: 4px; font-size: 0.75rem; font-weight: 700; color: var(--primary); box-shadow: 0 1px 2px rgba(0,0,0,0.05); cursor: pointer;">Week</button>
                <button style="border: none; background: transparent; padding: 0.25rem 0.75rem; font-size: 0.75rem; font-weight: 700; color: var(--text-muted); cursor: pointer;">Month</button>
                </div>
                </div>
                <div class="chart-area">
                <div class="chart-bar" style="height: 60%;"><div class="chart-bar-fill" style="height: 70%;"></div></div>
                <div class="chart-bar" style="height: 80%;"><div class="chart-bar-fill" style="height: 85%;"></div></div>
                <div class="chart-bar" style="height: 50%;"><div class="chart-bar-fill" style="height: 60%;"></div></div>
                <div class="chart-bar" style="height: 90%;"><div class="chart-bar-fill" style="height: 95%;"></div></div>
                <div class="chart-bar" style="height: 75%;"><div class="chart-bar-fill" style="height: 80%;"></div></div>
                <div class="chart-bar" style="height: 65%;"><div class="chart-bar-fill" style="height: 70%;"></div></div>
                <div class="chart-bar" style="height: 85%;"><div class="chart-bar-fill" style="height: 90%;"></div></div>
                </div>
                </div>
                <!-- Active Tasks -->
                <div class="card-container" style="display: flex; flex-direction: column;">
                <div class="card-header">
                <h2 class="card-title">Active Tasks</h2>
                <a href="#" style="color: var(--primary); text-decoration: none; font-size: 0.875rem; font-weight: 600;">View All</a>
                </div>
                <div class="task-list">
                <div class="task-item">
                <input class="task-checkbox" type="checkbox"/>
                <div class="task-info">
                <p>Review quarterly revenue audit</p>
                <span>Due today · 2:00 PM</span>
                </div>
                </div>
                <div class="task-item">
                <input class="task-checkbox" type="checkbox"/>
                <div class="task-info">
                <p>Update performance metrics</p>
                <span>Due tomorrow · 10:00 AM</span>
                </div>
                </div>
                <div class="task-item task-done">
                <input checked="" class="task-checkbox" type="checkbox"/>
                <div class="task-info">
                <p>Finalize documentation</p>
                <span>Completed</span>
                </div>
                </div>
                </div>
                <div style="margin-top: auto; padding-top: 1rem; border-top: 1px solid var(--border-color);">
                <div style="background: var(--surface-container); padding: 0.75rem; border-radius: var(--radius-md); display: flex; align-items: center; justify-content: space-between;">
                <div style="display: flex; margin-left: 8px;">
                <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuDLOD1roLInyG3xJ490FNYv4UW3t490noArTcjmLReT4UddB50wR5gxs3yRu0RXvpJMfA3kVpb0iMfv3hx_L0vWN1gpVVxVM7ctu7DqmVzJzxN3NGHdCiCZSGvJsEnPjTtZNRBJhET2PXUgmbvQjWG9tuT2WK6NuWkXonE5nOphBIy-AHZxIwqhFC6SsN95CWMgy0fHb3sj32Jn29MW2qRGGvuSS-imAwGF7QkEw96Ui6ApzvDTFc9dqqiQMp9_jUJAPuFGCU7CjA" style="width: 28px; height: 28px; border-radius: 50%; border: 2px solid white; margin-left: -8px;"/>
                <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuAbs6A41l5QU47IAhQ5Fb_g7-9xOdr_BD3zS1lf3mFHm9ugUZkGybhAa6_RlkfuGOZObTOkEtlnBTJIUUboer2wNTSQNLR9t7rc92K76geSvyGNoh-J961RwbSN0dfNUUp61s7FUyEwkTLu-e6auNHKALTZcvQIAcRRoi9pv9Ox5hApGdUiFFIdQoQhnFs6fPR9HJN3jQ9NM30LStM99KgVkoLwUku0UgF37B_miRZGOVlFy_7PQD3UOMTj43sOWVnfYNWX1ZpCOA" style="width: 28px; height: 28px; border-radius: 50%; border: 2px solid white; margin-left: -8px;"/>
                <div style="width: 28px; height: 28px; border-radius: 50%; border: 2px solid white; background: #cbd5e1; display: flex; align-items: center; justify-content: center; font-size: 10px; font-weight: 700; margin-left: -8px;">+4</div>
                </div>
                <span style="font-size: 0.75rem; font-weight: 600; color: var(--text-muted);">Team sync in 45m</span>
                </div>
                </div>
                </div>
                </div>
                <!-- Recent Transactions -->
                <section class="table-section">
                <div class="table-controls">
                <h2 class="card-title">Recent Transactions</h2>
                <div class="table-actions">
                <button><span class="material-symbols-outlined" style="font-size: 1.2rem;">filter_list</span> Filter</button>
                <button class="btn-export"><span class="material-symbols-outlined" style="font-size: 1.2rem;">download</span> Export</button>
                </div>
                </div>
                <div class="table-wrapper">
                <table>
                <thead>
                <tr>
                <th>Order ID</th>
                <th>Customer</th>
                <th>Date</th>
                <th>Amount</th>
                <th>Status</th>
                <th style="text-align: right;">Action</th>
                </tr>
                </thead>
                <tbody>
                <tr>
                <td class="order-id">#ORD-2841</td>
                <td>
                <div class="customer-cell">
                <div class="customer-avatar">JS</div>
                <span style="font-weight: 500;">Julianne Smith</span>
                </div>
                </td>
                <td style="color: var(--text-muted);">Oct 24, 2023</td>
                <td style="font-weight: 600;">$1,250.00</td>
                <td><span class="status-pill status-pending">Pending</span></td>
                <td style="text-align: right;"><span class="material-symbols-outlined" style="color: var(--text-light); cursor: pointer;">more_vert</span></td>
                </tr>
                <tr>
                <td class="order-id">#ORD-2840</td>
                <td>
                <div class="customer-cell">
                <img class="customer-avatar" src="https://lh3.googleusercontent.com/aida-public/AB6AXuCXYAr_xJDK_lWtSplXCHp46WGaat43FoTgxj0Uqc_KEYKKD_9zccDQkRR7bBkADIVPYQkqhUeAuKjKDoLYHCaxUE-1gXNwame56YVF74vPve4Qda90x5cM5eqYmqmiQkQzELNbv_1ZWJr3pa5PIAyKPCqhO2knET_9A_gfy6QhKVJMxpsabMeRkG2Ues5FQZ0OMV28_KuYn5VmcA2-rjUybIWz1qVXRYawDXoB-DfGeG_xLu1t0QfKLytUrB1W43w7Nkl8A9cEQw" style="object-fit: cover;"/>
                <span style="font-weight: 500;">Arthur Morgan</span>
                </div>
                </td>
                <td style="color: var(--text-muted);">Oct 23, 2023</td>
                <td style="font-weight: 600;">$890.00</td>
                <td><span class="status-pill status-completed">Completed</span></td>
                <td style="text-align: right;"><span class="material-symbols-outlined" style="color: var(--text-light); cursor: pointer;">more_vert</span></td>
                </tr>
                <tr>
                <td class="order-id">#ORD-2839</td>
                <td>
                <div class="customer-cell">
                <div class="customer-avatar">LR</div>
                <span style="font-weight: 500;">Lana Rhodes</span>
                </div>
                </td>
                <td style="color: var(--text-muted);">Oct 23, 2023</td>
                <td style="font-weight: 600;">$45.00</td>
                <td><span class="status-pill status-cancelled">Cancelled</span></td>
                <td style="text-align: right;"><span class="material-symbols-outlined" style="color: var(--text-light); cursor: pointer;">more_vert</span></td>
                </tr>
                </tbody>
                </table>
                </div>
                <div class="pagination">
                <p style="font-size: 0.75rem; color: var(--text-muted);">Showing 3 of 128 orders</p>
                <div style="display: flex; gap: 0.5rem;">
                <button style="width: 28px; height: 28px; background: white; border: 1px solid var(--border-color); border-radius: 4px; display: flex; align-items: center; justify-content: center; cursor: pointer; color: var(--text-muted); opacity: 0.5;"><span class="material-symbols-outlined" style="font-size: 1rem;">chevron_left</span></button>
                <button style="width: 28px; height: 28px; background: var(--primary); border: none; border-radius: 4px; color: white; font-weight: 700; font-size: 0.75rem; cursor: pointer;">1</button>
                <button style="width: 28px; height: 28px; background: white; border: 1px solid var(--border-color); border-radius: 4px; font-weight: 700; font-size: 0.75rem; cursor: pointer;">2</button>
                <button style="width: 28px; height: 28px; background: white; border: 1px solid var(--border-color); border-radius: 4px; display: flex; align-items: center; justify-content: center; cursor: pointer; color: var(--text-muted);"><span class="material-symbols-outlined" style="font-size: 1rem;">chevron_right</span></button>
                </div>
                </div>
                </section>
                <div id="chat-bubble">💬</div>

                <div id="chat-container" class="hidden">
                    <div class="chat-header">
                        <span>AI Chat</span>
                        <button id="chat-close">✖</button>
                    </div>

                    <div id="chat-history"></div>

                    <div class="chat-input">
                        <textarea id="chat-text" placeholder="Tulis pesan..."></textarea>
                        <button id="chat-send">Kirim</button>
                    </div>
                </div>
                <footer>
                            © 2023 Executive Curator System. All architectural rights reserved. Built for precision.
                        </footer>
                </div>
                <script src="/scripts/oams.js"></script>
                </body></html>
                """;
            return Results.Content(html, "text/html");
        }
    }
}
