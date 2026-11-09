using System.Drawing.Text;

namespace RandevuPlanlayici;

internal sealed class AgendaForm : Form
{
    private static readonly Color Lavender = ColorTranslator.FromHtml("#F3EDF8");
    private static readonly Color Paper = ColorTranslator.FromHtml("#FDFBFF");
    private static readonly Color Ink = ColorTranslator.FromHtml("#55446B");
    private static readonly Color Ice = ColorTranslator.FromHtml("#E2EFF6");
    private static readonly Color Blush = ColorTranslator.FromHtml("#F4E4EE");

    private readonly PrivateFontCollection fonts = new();
    private readonly Dictionary<DateOnly, List<Appointment>> days = new();
    private readonly DateTimePicker date = new() { Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker start = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true };
    private readonly NumericUpDown duration = new() { Minimum = 5, Maximum = 540, Value = 45, Increment = 5 };
    private readonly TextBox title = new() { PlaceholderText = "Örn. Müşteri görüşmesi", MaxLength = 50 };
    private readonly ListView agenda = new() { View = View.Details, FullRowSelect = true, MultiSelect = false, UseCompatibleStateImageBehavior = false };
    private readonly Label notice = new() { AutoSize = true, MaximumSize = new Size(290, 0), Padding = new Padding(8) };
    private readonly Button suggestion = new() { Visible = false };
    private int? suggestedStart;

    public AgendaForm()
    {
        Text = "Randevu Planlayıcı";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(920, 550);
        MinimumSize = new Size(760, 510);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Lavender;
        ForeColor = Ink;

        string path = Path.Combine(AppContext.BaseDirectory, "Fonts", "Figtree-Regular.ttf");
        if (File.Exists(path))
        {
            fonts.AddFontFile(path);
            if (fonts.Families.Length > 0)
                Font = new Font(fonts.Families[0], 11);
        }

        date.Value = DateTime.Today;
        start.Value = DateTime.Today.AddHours(10).AddMinutes(30);
        days[SelectedDay] = new()
        {
            new("Tanışma görüşmesi", 570, 30),
            new("Ekip görüşmesi", 630, 60),
            new("Gün sonu değerlendirmesi", 960, 30),
        };

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 2, RowCount = 2 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var heading = Heading("Randevu Planlayıcı", 24);
        root.Controls.Add(heading, 0, 0);
        root.SetColumnSpan(heading, 2);
        Controls.Add(root);

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Paper, Padding = new Padding(14), ColumnCount = 1, RowCount = 3 };
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.Controls.Add(Heading("Günlük ajanda", 15), 0, 0);
        date.Width = 145;
        date.Margin = new Padding(3, 9, 3, 12);
        left.Controls.Add(date, 0, 1);
        agenda.Dock = DockStyle.Fill;
        agenda.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        agenda.Columns.Add("Saat", 115);
        agenda.Columns.Add("Randevu", 225);
        left.Controls.Add(agenda, 0, 2);
        root.Controls.Add(left, 0, 1);

        var right = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Paper, Padding = new Padding(14), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        right.Controls.Add(Heading("Yeni randevu", 15));
        AddField(right, "Randevu adı", title);
        AddField(right, "Başlangıç", start);
        AddField(right, "Süre (dakika)", duration);

        var save = new Button { Text = "Randevuyu kaydet" };
        StyleButton(save, Ink, Color.White);
        StyleButton(suggestion, Ice, Ink);
        right.Controls.Add(save);
        right.Controls.Add(notice);
        right.Controls.Add(suggestion);
        root.Controls.Add(right, 1, 1);
        AcceptButton = save;

        save.Click += (_, _) => Save();
        suggestion.Click += (_, _) =>
        {
            if (suggestedStart is int minute)
            {
                start.Value = DateTime.Today.AddMinutes(minute);
                notice.Text = "Önerilen saat forma aktarıldı. Kaydederek onaylayabilirsin.";
                notice.BackColor = Ice;
            }
        };
        date.ValueChanged += (_, _) => ResetFeedback();
        start.ValueChanged += (_, _) => ResetFeedback();
        duration.ValueChanged += (_, _) => ResetFeedback();
        RenderAgenda();
    }

    private DateOnly SelectedDay => DateOnly.FromDateTime(date.Value);
    private List<Appointment> CurrentAppointments => days.TryGetValue(SelectedDay, out var items) ? items : new();
    private static string Clock(int minute) => $"{minute / 60:00}:{minute % 60:00}";

    private Label Heading(string text, float size) =>
        new() { Text = text, AutoSize = true, Font = new Font(Font.FontFamily, size), ForeColor = Ink };

    private static void AddField(FlowLayoutPanel panel, string label, Control input)
    {
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 8, 3, 1) });
        input.Width = 260;
        input.AccessibleName = label;
        panel.Controls.Add(input);
    }

    private static void StyleButton(Button button, Color background, Color foreground)
    {
        button.AutoSize = true;
        button.Padding = new Padding(10, 4, 10, 4);
        button.Margin = new Padding(3, 9, 3, 3);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = background;
        button.ForeColor = foreground;
    }

    private void Save()
    {
        ResetFeedback();
        int minute = (int)start.Value.TimeOfDay.TotalMinutes;
        int length = (int)duration.Value;
        if (string.IsNullOrWhiteSpace(title.Text))
        {
            ShowNotice("Randevu adını gir.", Blush);
            title.Focus();
            return;
        }
        if (!Planner.FitsWorkday(minute, length))
        {
            ShowNotice("Randevu 09:00–18:00 arasına sığmalı.", Blush);
            return;
        }

        var items = CurrentAppointments;
        var conflict = Planner.FindConflict(items, minute, length);
        if (conflict is not null)
        {
            suggestedStart = Planner.FirstAvailable(items, minute, length);
            ShowNotice(suggestedStart.HasValue
                ? $"Bu saat {conflict.Title} ile çakışıyor."
                : $"Bu saat {conflict.Title} ile çakışıyor. Günün kalanında uygun aralık yok.", Blush);
            if (suggestedStart is int available)
            {
                suggestion.Text = $"{Clock(available)} saatini seç";
                suggestion.Visible = true;
            }
            RenderAgenda(conflict);
            return;
        }

        items.Add(new Appointment(title.Text.Trim(), minute, length));
        days[SelectedDay] = items;
        title.Clear();
        ShowNotice("Randevu kaydedildi.", Ice);
        RenderAgenda();
    }

    private void ResetFeedback()
    {
        suggestedStart = null;
        suggestion.Visible = false;
        notice.Text = "";
        notice.BackColor = Paper;
        RenderAgenda();
    }

    private void ShowNotice(string message, Color color)
    {
        notice.Text = message;
        notice.BackColor = color;
    }

    private void RenderAgenda(Appointment? conflict = null)
    {
        agenda.BeginUpdate();
        agenda.Items.Clear();
        var rows = CurrentAppointments.Select(item =>
            (item.Start, item.End, item.Title, Color: item == conflict ? Blush : Paper)).ToList();
        if (suggestedStart is int available)
            rows.Add((available, available + (int)duration.Value, "Önerilen boş aralık", Ice));
        foreach (var row in rows.OrderBy(row => row.Start))
        {
            var entry = new ListViewItem($"{Clock(row.Start)}–{Clock(row.End)}") { BackColor = row.Color };
            entry.SubItems.Add(row.Title);
            agenda.Items.Add(entry);
        }
        if (rows.Count == 0)
            agenda.Items.Add(new ListViewItem(new[] { "—", "Bu gün için randevu yok" }));
        agenda.EndUpdate();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            fonts.Dispose();
    }
}
