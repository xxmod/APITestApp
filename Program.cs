using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace APITestApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

// 路径控件（只有值，没有键）
public class PathControl : UserControl
{
    public TextBox ValueTextBox { get; private set; } = null!;
    public Button DeleteButton { get; private set; } = null!;

    public event EventHandler? DeleteRequested;

    public string Value
    {
        get => ValueTextBox.Text;
        set => ValueTextBox.Text = value;
    }

    public PathControl()
    {
        InitializePathControl();
    }

    private void InitializePathControl()
    {
        this.Height = 35;
        this.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 2, 0, 2),
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

        ValueTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "路径段（如：api, users, 123）",
            Font = new Font("Segoe UI", 9F),
        };

        DeleteButton = new Button
        {
            Text = "×",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            UseVisualStyleBackColor = false,
        };
        DeleteButton.FlatAppearance.BorderSize = 0;
        DeleteButton.Click += (s, e) => DeleteRequested?.Invoke(this, EventArgs.Empty);

        layout.Controls.Add(ValueTextBox, 0, 0);
        layout.Controls.Add(DeleteButton, 1, 0);

        this.Controls.Add(layout);
    }
}

// 路径管理面板
public class PathPanel : UserControl
{
    // 使用 Panel + Dock=Top 代替 FlowLayoutPanel，避免 FlowLayoutPanel 忽略子控件 Dock 导致宽度为最小值的问题
    private Panel contentPanel = null!;
    private Button addButton = null!;
    private List<PathControl> pathControls = new();

    public List<string> GetPaths()
    {
        return pathControls
            .Select(c => c.Value.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
    }

    public void SetPaths(List<string> paths)
    {
        ClearAll();
        foreach (var path in paths)
        {
            AddPath(path);
        }
        if (!paths.Any())
        {
            AddPath("");
        }
    }

    public PathPanel()
    {
        InitializePathPanel();
    }

    private void InitializePathPanel()
    {
        var mainLayout = new Panel { Dock = DockStyle.Fill };

        addButton = new Button
        {
            Text = "+ 添加路径",
            Height = 35,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F),
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += (s, e) => AddPath("");

        contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 5, 0, 5),
        };

        mainLayout.Controls.Add(addButton);
        mainLayout.Controls.Add(contentPanel);

        this.Controls.Add(mainLayout);

        // 添加第一个空白项
        AddPath("");
    }

    private void AddPath(string path)
    {
        var control = new PathControl { Value = path };
        control.DeleteRequested += (s, e) =>
        {
            if (pathControls.Count > 1)
            {
                RemovePath(control);
            }
        };

        pathControls.Add(control);

        control.Dock = DockStyle.Top; // 在 Panel 内可正确拉伸
        control.Height = 40;
        control.Margin = new Padding(0, 2, 0, 2);
        contentPanel.Controls.Add(control);
        // 维持添加顺序为从上到下（Dock=Top 默认新控件会插到最上方，需要 SendToBack）
        control.SendToBack();

        UpdateLayout();
    }

    private void RemovePath(PathControl control)
    {
        pathControls.Remove(control);
        contentPanel.Controls.Remove(control);
        control.Dispose();
        UpdateLayout();
    }

    private void ClearAll()
    {
        foreach (var control in pathControls.ToList())
        {
            contentPanel.Controls.Remove(control);
            control.Dispose();
        }
        pathControls.Clear();
    }

    private void UpdateLayout()
    {
        this.Invalidate();
    }
}

// 键值对控件
public class KeyValueControl : UserControl
{
    public TextBox KeyTextBox { get; private set; } = null!;
    public TextBox ValueTextBox { get; private set; } = null!;
    public Button DeleteButton { get; private set; } = null!;

    public event EventHandler? DeleteRequested;

    public string Key
    {
        get => KeyTextBox.Text;
        set => KeyTextBox.Text = value;
    }

    public string Value
    {
        get => ValueTextBox.Text;
        set => ValueTextBox.Text = value;
    }

    public KeyValueControl()
    {
        InitializeKeyValueControl();
    }

    private void InitializeKeyValueControl()
    {
        this.Height = 35;
        this.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 2, 0, 2),
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

        KeyTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "键",
            Font = new Font("Segoe UI", 9F),
        };

        ValueTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "值",
            Font = new Font("Segoe UI", 9F),
        };

        DeleteButton = new Button
        {
            Text = "×",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(220, 53, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            UseVisualStyleBackColor = false,
        };
        DeleteButton.FlatAppearance.BorderSize = 0;
        DeleteButton.Click += (s, e) => DeleteRequested?.Invoke(this, EventArgs.Empty);

        layout.Controls.Add(KeyTextBox, 0, 0);
        layout.Controls.Add(ValueTextBox, 1, 0);
        layout.Controls.Add(DeleteButton, 2, 0);

        this.Controls.Add(layout);
    }
}

// 键值对管理面板
public class KeyValuePanel : UserControl
{
    // 同上，改为普通 Panel，保证子控件宽度填满
    private Panel contentPanel = null!;
    private Button addButton = null!;
    private List<KeyValueControl> keyValueControls = new();

    public string Title { get; set; } = "键值对";

    public Dictionary<string, string> GetValues()
    {
        var result = new Dictionary<string, string>();
        foreach (var control in keyValueControls)
        {
            if (!string.IsNullOrWhiteSpace(control.Key))
            {
                result[control.Key] = control.Value;
            }
        }
        return result;
    }

    public void SetValues(Dictionary<string, string> values)
    {
        ClearAll();
        foreach (var kvp in values)
        {
            AddKeyValue(kvp.Key, kvp.Value);
        }
        if (!values.Any())
        {
            AddKeyValue("", "");
        }
    }

    public KeyValuePanel()
    {
        InitializeKeyValuePanel();
    }

    private void InitializeKeyValuePanel()
    {
        var mainLayout = new Panel { Dock = DockStyle.Fill };

        addButton = new Button
        {
            Text = "+ 添加",
            Height = 35,
            Dock = DockStyle.Bottom,
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F),
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += (s, e) => AddKeyValue("", "");

        contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(0, 5, 0, 5),
        };

        mainLayout.Controls.Add(addButton);
        mainLayout.Controls.Add(contentPanel);

        this.Controls.Add(mainLayout);

        // 添加第一个空白项
        AddKeyValue("", "");
    }

    private void AddKeyValue(string key, string value)
    {
        var control = new KeyValueControl { Key = key, Value = value };
        control.DeleteRequested += (s, e) =>
        {
            if (keyValueControls.Count > 1)
            {
                RemoveKeyValue(control);
            }
        };

        keyValueControls.Add(control);

        control.Dock = DockStyle.Top;
        control.Height = 40;
        control.Margin = new Padding(0, 2, 0, 2);
        contentPanel.Controls.Add(control);
        control.SendToBack();

        UpdateLayout();
    }

    private void RemoveKeyValue(KeyValueControl control)
    {
        keyValueControls.Remove(control);
        contentPanel.Controls.Remove(control);
        control.Dispose();
        UpdateLayout();
    }

    private void ClearAll()
    {
        foreach (var control in keyValueControls.ToList())
        {
            contentPanel.Controls.Remove(control);
            control.Dispose();
        }
        keyValueControls.Clear();
    }

    private void UpdateLayout()
    {
        this.Invalidate();
    }
}

public partial class MainForm : Form
{
    private TextBox txtApiUrl = null!;
    private PathPanel pathPanel = null!;
    private KeyValuePanel paramPanel = null!;
    private ComboBox cmbMethod = null!;
    private KeyValuePanel headerPanel = null!;
    private KeyValuePanel cookiePanel = null!;
    private RichTextBox txtResponse = null!;
    private RichTextBox txtCookieChanges = null!;
    private Button btnSend = null!;
    private Panel leftPanel = null!;
    private Panel rightPanel = null!;

    private readonly HttpClient httpClient;
    private readonly CookieContainer cookieContainer;
    private Dictionary<string, string> lastCookies = new();

    // --- 新增字段，用于动态放大行 ---
    private TableLayoutPanel leftMainLayout = null!;
    private readonly Dictionary<int, int> originalRowHeights = new();
    private readonly Dictionary<int, Control> sectionControlByRow = new();
    private int? activeEnlargedRow = null;
    private readonly int enlargePercent = 140; // 放大到 140%（可调整）

    public MainForm()
    {
        cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        httpClient = new HttpClient(handler);

        InitializeComponent();
        LoadInitialData();
    }

    private void InitializeComponent()
    {
        this.Text = "API 测试工具 - 现代版";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 9F);
        this.BackColor = Color.FromArgb(240, 240, 240);
        this.MinimumSize = new Size(1200, 700);

        // 创建主面板
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10),
        };

        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        // 左侧面板 - 请求配置
        leftPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            Margin = new Padding(5),
        };

        var leftScrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.White,
        };

        CreateLeftPanelContent(leftScrollPanel);
        leftPanel.Controls.Add(leftScrollPanel);
        mainPanel.Controls.Add(leftPanel, 0, 0);

        // 右侧面板 - 响应显示（上下分割）
        rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20),
            Margin = new Padding(5),
        };

        CreateRightPanelContent(rightPanel);
        mainPanel.Controls.Add(rightPanel, 1, 0);

        this.Controls.Add(mainPanel);
    }

    private void CreateLeftPanelContent(Panel parent)
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            AutoScroll = true,
            Padding = new Padding(10),
        };

        // 保存引用以便后续动态调整高度
        leftMainLayout = mainLayout;

        // 设置行样式
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // 0 标题
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // 1 URL方法
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // 2 路径
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // 3 参数
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // 4 请求头
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // 5 Cookie
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70)); // 6 按钮

        // 记录原始高度
        for (int i = 0; i < mainLayout.RowCount; i++)
        {
            originalRowHeights[i] = (int)mainLayout.RowStyles[i].Height;
        }

        // 标题
        var titleLabel = new Label
        {
            Text = "🚀 API 请求配置",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        mainLayout.Controls.Add(titleLabel, 0, 0);

        // URL 和方法配置
        var urlMethodPanel = CreateUrlMethodPanel();
        urlMethodPanel.Dock = DockStyle.Fill;
        mainLayout.Controls.Add(urlMethodPanel, 0, 1);
        // 记录并绑定焦点事件（所在行索引 = 1）
        sectionControlByRow[1] = urlMethodPanel;
        AttachFocusHandlersToSection(urlMethodPanel, 1);

        // 路径配置
        var pathGroup = CreatePathGroup();
        pathGroup.Dock = DockStyle.Fill;
        mainLayout.Controls.Add(pathGroup, 0, 2);
        sectionControlByRow[2] = pathGroup;
        AttachFocusHandlersToSection(pathGroup, 2);

        // 参数配置 (row 3)
        var paramGroup = CreateSectionGroup("🔧 查询参数", out paramPanel, 3);
        paramGroup.Dock = DockStyle.Fill;
        mainLayout.Controls.Add(paramGroup, 0, 3);
        sectionControlByRow[3] = paramGroup;
        AttachFocusHandlersToSection(paramGroup, 3);

        // 请求头配置 (row 4)
        var headerGroup = CreateSectionGroup("📋 请求头", out headerPanel, 4);
        headerGroup.Dock = DockStyle.Fill;
        mainLayout.Controls.Add(headerGroup, 0, 4);
        sectionControlByRow[4] = headerGroup;
        AttachFocusHandlersToSection(headerGroup, 4);

        // Cookie 配置 (row 5)
        var cookieGroup = CreateSectionGroup("🍪 Cookie", out cookiePanel, 5);
        cookieGroup.Dock = DockStyle.Fill;
        mainLayout.Controls.Add(cookieGroup, 0, 5);
        sectionControlByRow[5] = cookieGroup;
        AttachFocusHandlersToSection(cookieGroup, 5);

        // 发送按钮
        var buttonPanel = CreateSendButtonPanel();
        buttonPanel.Dock = DockStyle.Fill;
        mainLayout.Controls.Add(buttonPanel, 0, 6);

        parent.Controls.Add(mainLayout);
    }

    private GroupBox CreatePathGroup()
    {
        var group = new GroupBox
        {
            Text = "📁 路径配置",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Height = 150,
            Padding = new Padding(10),
            Margin = new Padding(0, 5, 0, 5),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(52, 58, 64),
        };

        pathPanel = new PathPanel { Dock = DockStyle.Fill };
        group.Controls.Add(pathPanel);
        return group;
    }

    private Panel CreateUrlMethodPanel()
    {
        var panel = new Panel
        {
            Height = 60,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(15, 10, 15, 10),
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        // URL 标签
        var urlLabel = new Label
        {
            Text = "URL:",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Fill,
        };

        // URL 输入框
        txtApiUrl = new TextBox
        {
            Text = "http://localhost:3001",
            Font = new Font("Segoe UI", 10F),
            Dock = DockStyle.Fill,
            Margin = new Padding(10, 5, 10, 5),
        };

        // 方法选择
        cmbMethod = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 5, 0, 5),
        };
        cmbMethod.Items.AddRange(new[] { "GET", "POST", "PUT", "DELETE", "PATCH" });
        cmbMethod.SelectedIndex = 0;

        // 设置方法选择框的颜色
        cmbMethod.BackColor = Color.FromArgb(0, 123, 255);
        cmbMethod.ForeColor = Color.White;

        layout.Controls.Add(urlLabel, 0, 0);
        layout.Controls.Add(txtApiUrl, 1, 0);
        layout.Controls.Add(cmbMethod, 2, 0);

        panel.Controls.Add(layout);

        // 也为 URL 和 方法输入框单独绑定聚焦事件（以提高响应速度）
        AttachFocusHandlersToSection(panel, 1);

        return panel;
    }

    // 修改：增加 rowIndex 参数，用于记录每个区所属的行
    private GroupBox CreateSectionGroup(string title, out KeyValuePanel kvPanel, int rowIndex)
    {
        var group = new GroupBox
        {
            Text = title,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Height = 150,
            Padding = new Padding(10),
            Margin = new Padding(0, 5, 0, 5),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(52, 58, 64),
        };

        kvPanel = new KeyValuePanel { Dock = DockStyle.Fill };
        group.Controls.Add(kvPanel);

        // 记录并绑定（如果此方法也单独调用则确保绑定）
        sectionControlByRow[rowIndex] = group;
        AttachFocusHandlersToSection(group, rowIndex);

        return group;
    }

    private Panel CreateSendButtonPanel()
    {
        var panel = new Panel { Height = 60, Padding = new Padding(0, 10, 0, 0) };

        btnSend = new Button
        {
            Text = "🚀 发送请求",
            Height = 45,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(40, 167, 69),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            UseVisualStyleBackColor = false,
        };
        btnSend.FlatAppearance.BorderSize = 0;
        btnSend.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 142, 59);
        btnSend.Click += BtnSend_Click;

        panel.Controls.Add(btnSend);
        return panel;
    }

    private void CreateRightPanelContent(Panel parent)
    {
        var titleLabel = new Label
        {
            Text = "📊 响应结果",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            Height = 40,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleLeft,
        };

        // 创建上下分割的面板
        var splitter = new SplitContainer
        {
            Orientation = Orientation.Horizontal,
            Dock = DockStyle.Fill,
            SplitterDistance = 300,
            Panel1MinSize = 150,
            Panel2MinSize = 100,
        };

        // 上半部分 - 响应内容
        var responseGroup = new GroupBox
        {
            Text = "📄 响应内容",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Padding = new Padding(10),
        };

        txtResponse = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cascadia Code", 9F),
            ReadOnly = true,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(220, 220, 220),
            BorderStyle = BorderStyle.None,
        };
        responseGroup.Controls.Add(txtResponse);
        splitter.Panel1.Controls.Add(responseGroup);

        // 下半部分 - Cookie 变化
        var cookieGroup = new GroupBox
        {
            Text = "🍪 Cookie 变化",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Padding = new Padding(10),
        };

        txtCookieChanges = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Cascadia Code", 9F),
            ReadOnly = true,
            BackColor = Color.FromArgb(40, 44, 52),
            ForeColor = Color.FromArgb(171, 178, 191),
            BorderStyle = BorderStyle.None,
        };
        cookieGroup.Controls.Add(txtCookieChanges);
        splitter.Panel2.Controls.Add(cookieGroup);

        parent.Controls.Add(titleLabel);
        parent.Controls.Add(splitter);
    }

    private void LoadInitialData()
    {
        // 设置默认的路径
        pathPanel.SetPaths(new List<string> { "api", "test" });

        // 设置默认的参数
        paramPanel.SetValues(
            new Dictionary<string, string>
            {
                { "type", "1" },
                { "number", "2" },
                { "key", "3" },
            }
        );

        // 设置默认的请求头
        headerPanel.SetValues(
            new Dictionary<string, string>
            {
                { "User-Agent", "APITestApp/2.0" },
                { "Accept", "application/json" },
            }
        );

        // 设置默认的Cookie
        cookiePanel.SetValues(
            new Dictionary<string, string> { { "session", "example_session_id" } }
        );
    }

    private async void BtnSend_Click(object? sender, EventArgs e)
    {
        try
        {
            btnSend.Enabled = false;
            btnSend.Text = "🔄 发送中...";

            // 构建完整 URL
            var baseUrl = txtApiUrl.Text.TrimEnd('/');
            var paths = pathPanel.GetPaths();
            var parameters = paramPanel.GetValues();

            var fullUrl = baseUrl;

            // 添加路径
            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (!path.StartsWith("/"))
                        fullUrl += "/";
                    fullUrl += path.TrimStart('/');
                }
            }

            // 添加查询参数
            if (parameters.Any(p => !string.IsNullOrWhiteSpace(p.Key)))
            {
                var queryParams = parameters
                    .Where(p => !string.IsNullOrWhiteSpace(p.Key))
                    .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}")
                    .ToArray();

                if (queryParams.Any())
                {
                    fullUrl += "?" + string.Join("&", queryParams);
                }
            }

            txtResponse.Text =
                $"🚀 请求 URL: {fullUrl}\n📋 请求方法: {cmbMethod.Text}\n⏰ 时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n";

            // 设置 Cookies
            SetCookiesFromPanel(fullUrl);

            // 发送请求
            var result = await SendRequest(cmbMethod.Text, fullUrl);

            // 显示响应
            txtResponse.Text += result;

            // 检查 Cookie 变化
            CheckCookieChanges(fullUrl);
        }
        catch (Exception ex)
        {
            txtResponse.Text += $"❌ 错误: {ex.Message}";
        }
        finally
        {
            btnSend.Enabled = true;
            btnSend.Text = "🚀 发送请求";
        }
    }

    private async Task<string> SendRequest(string method, string url)
    {
        try
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            // 添加请求头
            var headers = headerPanel.GetValues();
            foreach (var header in headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
            {
                try
                {
                    if (header.Key.ToLower() == "content-type")
                    {
                        continue;
                    }
                    request.Headers.Add(header.Key, header.Value);
                }
                catch
                {
                    // 忽略无效的请求头
                }
            }

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = new StringBuilder();
            result.AppendLine($"✅ 状态码: {(int)response.StatusCode} {response.ReasonPhrase}");
            result.AppendLine($"⏱️  响应时间: {DateTime.Now:HH:mm:ss}");
            result.AppendLine("\n📋 响应头:");
            foreach (var header in response.Headers)
            {
                result.AppendLine($"   {header.Key}: {string.Join(", ", header.Value)}");
            }
            foreach (var header in response.Content.Headers)
            {
                result.AppendLine($"   {header.Key}: {string.Join(", ", header.Value)}");
            }

            result.AppendLine($"\n📄 响应内容:");
            result.AppendLine(new string('=', 50));

            // 尝试格式化JSON
            try
            {
                var jsonObj = JsonConvert.DeserializeObject(responseContent);
                var formattedJson = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                result.AppendLine(formattedJson);
            }
            catch
            {
                result.AppendLine(responseContent);
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ 请求失败: {ex.Message}";
        }
    }

    private void SetCookiesFromPanel(string url)
    {
        try
        {
            var cookies = cookiePanel.GetValues();
            if (cookies.Any(c => !string.IsNullOrWhiteSpace(c.Key)))
            {
                var uri = new Uri(url);
                foreach (var cookie in cookies.Where(c => !string.IsNullOrWhiteSpace(c.Key)))
                {
                    var cookieObj = new Cookie(cookie.Key, cookie.Value, "/", uri.Host);
                    cookieContainer.Add(cookieObj);
                }
            }
        }
        catch (Exception ex)
        {
            txtResponse.Text += $"❌ Cookie 设置错误: {ex.Message}\n";
        }
    }

    private void CheckCookieChanges(string url)
    {
        try
        {
            var uri = new Uri(url);
            var currentCookies = new Dictionary<string, string>();

            foreach (Cookie cookie in cookieContainer.GetCookies(uri))
            {
                currentCookies[cookie.Name] = cookie.Value;
            }

            var changes = new List<string>();

            // 检查新增或修改的 cookies
            foreach (var current in currentCookies)
            {
                if (!lastCookies.ContainsKey(current.Key))
                {
                    changes.Add($"🆕 新增: {current.Key} = {current.Value}");
                }
                else if (lastCookies[current.Key] != current.Value)
                {
                    changes.Add(
                        $"🔄 修改: {current.Key} = {current.Value} (原值: {lastCookies[current.Key]})"
                    );
                }
            }

            // 检查删除的 cookies
            foreach (var last in lastCookies)
            {
                if (!currentCookies.ContainsKey(last.Key))
                {
                    changes.Add($"🗑️ 删除: {last.Key}");
                }
            }

            // 显示变化
            if (changes.Any())
            {
                txtCookieChanges.Text =
                    $"🍪 Cookie 变化检测 - {DateTime.Now:HH:mm:ss}\n"
                    + new string('=', 40)
                    + "\n"
                    + string.Join("\n", changes)
                    + "\n\n"
                    + "📋 当前所有 Cookie:\n"
                    + string.Join("\n", currentCookies.Select(c => $"   {c.Key} = {c.Value}"));

                // 更新 Cookie 面板
                cookiePanel.SetValues(currentCookies);
            }
            else
            {
                txtCookieChanges.Text =
                    $"✅ 没有 Cookie 变化 - {DateTime.Now:HH:mm:ss}\n\n"
                    + "📋 当前所有 Cookie:\n"
                    + string.Join("\n", currentCookies.Select(c => $"   {c.Key} = {c.Value}"));
            }

            // 更新 lastCookies
            lastCookies = new Dictionary<string, string>(currentCookies);
        }
        catch (Exception ex)
        {
            txtCookieChanges.Text = $"❌ 检查 Cookie 变化时出错: {ex.Message}";
        }
    }

    // ----------------- 放大 / 恢复 相关方法 -----------------

    private void AttachFocusHandlersToSection(Control sectionControl, int rowIndex)
    {
        // 递归为该区内的所有子控件添加 Enter/Leave（GotFocus/LostFocus 也可以）
        void AddHandlers(Control c)
        {
            c.Enter += (s, e) => EnlargeSection(rowIndex);
            c.Leave += (s, e) =>
            {
                // 用 BeginInvoke 延后判断焦点是否真的移出该区（避免同一区域内控件切换导致误恢复）
                this.BeginInvoke(
                    new Action(() =>
                    {
                        var focused = GetDeepFocusedControl();
                        if (focused == null || !IsControlInContainer(focused, sectionControl))
                        {
                            RestoreSection(rowIndex);
                        }
                    })
                );
            };

            foreach (Control child in c.Controls)
            {
                AddHandlers(child);
            }
        }

        AddHandlers(sectionControl);
    }

    private void EnlargeSection(int rowIndex)
    {
        if (activeEnlargedRow == rowIndex)
            return;

        // 先恢复之前的
        if (activeEnlargedRow != null)
        {
            RestoreSection(activeEnlargedRow.Value);
        }

        if (!originalRowHeights.ContainsKey(rowIndex))
            return;

        var original = originalRowHeights[rowIndex];
        var newHeight = Math.Max(original, original * enlargePercent / 100);

        leftMainLayout.RowStyles[rowIndex].SizeType = SizeType.Absolute;
        leftMainLayout.RowStyles[rowIndex].Height = newHeight;

        // 高亮对应控件（若有）
        if (sectionControlByRow.TryGetValue(rowIndex, out var c))
        {
            c.BackColor = Color.FromArgb(235, 245, 255); // 轻微高亮
            c.Padding = new Padding(12); // 轻微内边距
        }

        activeEnlargedRow = rowIndex;
    }

    private void RestoreSection(int rowIndex)
    {
        if (!originalRowHeights.ContainsKey(rowIndex))
            return;

        // 如果当前活动放大行不是这个，就不用处理
        if (activeEnlargedRow != rowIndex)
            return;

        leftMainLayout.RowStyles[rowIndex].SizeType = SizeType.Absolute;
        leftMainLayout.RowStyles[rowIndex].Height = originalRowHeights[rowIndex];

        if (sectionControlByRow.TryGetValue(rowIndex, out var c))
        {
            c.BackColor = Color.White;
            c.Padding = new Padding(10);
        }

        activeEnlargedRow = null;
    }

    // 判断一个控件是否在容器（或其子孙）中
    private bool IsControlInContainer(Control child, Control container)
    {
        Control? cur = child;
        while (cur != null)
        {
            if (cur == container)
                return true;
            cur = cur.Parent;
        }
        return false;
    }

    // 深度获取当前焦点控件（遍历 ActiveControl 链）
    // 深度获取当前焦点控件（遍历 ContainerControl 的 ActiveControl 链）
    private Control? GetDeepFocusedControl()
    {
        // this.ActiveControl 在 Form (ContainerControl) 上可用
        Control? focused = this.ActiveControl;

        // 如果 focused 是 ContainerControl（有 ActiveControl 属性），就继续深入
        while (focused is ContainerControl container && container.ActiveControl != null)
        {
            focused = container.ActiveControl;
        }

        return focused;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            httpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}
