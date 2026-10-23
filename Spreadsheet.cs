using System.Numerics;

namespace Lab1.Core;

public sealed class Cell
{
    public string Raw = "";
    public Node? Ast;
    public FormulaException? Error;
    public object? Value;   
    internal int State;    

    public string Display => Error != null ? "#ПОМИЛКА" : Spreadsheet.Format(Value);
}

public sealed class Spreadsheet
{
    
    public bool StrictLogical { get; set; } = false;

    private readonly List<List<Cell>> _rows = new();
    public int Rows => _rows.Count;
    public int Cols { get; }

    public Cell this[int r, int c] => _rows[r][c];

    public Spreadsheet(int rows, int cols)
    {
        Cols = cols;
        for (int i = 0; i < rows; i++)
        {
            var row = new List<Cell>();
            for (int j = 0; j < cols; j++) row.Add(new Cell());
            _rows.Add(row);
        }
    }

  
    public void SetRaw(int r, int c, string text)
    {
        _rows[r][c].Raw = text;
        Recalculate();
    }

    public static string ColName(int c)
    {
        string s = "";
        c++;
        while (c > 0) { c--; s = (char)('A' + c % 26) + s; c /= 26; }
        return s;
    }

    public static string Address(int r, int c) => ColName(c) + (r + 1);

    public static bool TryParseAddress(string name, out int row, out int col)
    {
        row = col = 0;
        int i = 0;
        while (i < name.Length && char.IsAsciiLetter(name[i])) i++;
        if (i == 0 || i > 3 || i == name.Length) return false;
        if (!int.TryParse(name.AsSpan(i), out int r) || r < 1) return false;
        int cc = 0;
        for (int k = 0; k < i; k++) cc = cc * 26 + (char.ToUpperInvariant(name[k]) - 'A' + 1);
        row = r - 1;
        col = cc - 1;
        return true;
    }

    public static string Format(object? v) => v switch
    {
        null => "",
        bool b => b ? "ІСТИНА" : "ХИБНІСТЬ",
        BigInteger i => i.ToString(),
        _ => v.ToString() ?? ""
    };

    public void Recalculate()
    {
        foreach (var row in _rows)
            foreach (var cell in row)
            {
                cell.Ast = null; cell.Error = null; cell.Value = null; cell.State = 0;
                if (string.IsNullOrWhiteSpace(cell.Raw)) { cell.State = 2; continue; }
                try { cell.Ast = ExpressionParser.Parse(cell.Raw); }
                catch (FormulaException e) { cell.Error = e; cell.State = 2; }
            }

        foreach (var row in _rows)
            foreach (var cell in row)
                Resolve(cell, null);
    }

    private void Resolve(Cell cell, Node? from)
    {
        if (cell.State == 2) return;
        if (cell.State == 1)
            throw Err($"Циклічне посилання на клітинку {from?.Name}", from!, false);

        cell.State = 1;
        try
        {
            object v = Eval(cell.Ast!);
            if (StrictLogical && v is not bool)
                throw Err("Результат виразу має бути логічним", cell.Ast!, false);
            cell.Value = v;
        }
        catch (FormulaException e) { cell.Error = e; }
        cell.State = 2;
    }

    private static FormulaException Err(string msg, Node n, bool syntax) => new(msg, n.Pos, n.Len, syntax);

    private object Eval(Node n)
    {
        switch (n.Kind)
        {
            case NodeKind.Num: return n.Num;
            case NodeKind.Bool: return n.Bool;

            case NodeKind.Ref:
            {
                if (n.Row >= Rows || n.Col >= Cols)
                    throw Err($"Клітинка {n.Name} не існує в таблиці", n, false);
                var target = _rows[n.Row][n.Col];
                Resolve(target, n);
                if (target.Error != null)
                    throw Err($"Клітинка {n.Name} містить помилку", n, false);
                return target.Value ?? BigInteger.Zero; // порожня клітинка = 0
            }

            case NodeKind.Unary:
            {
                object v = Eval(n.A!);
                return n.Op switch
                {
                    Op.Not => !AsBool(v, n.A!, "not"),
                    Op.Minus => -AsInt(v, n.A!, "-"),
                    _ => AsInt(v, n.A!, "+")
                };
            }

            default:
            {
                object l = Eval(n.A!);
                object r = Eval(n.B!);
                switch (n.Op)
                {
                    case Op.And: return AsBool(l, n.A!, "and") & AsBool(r, n.B!, "and");
                    case Op.Or: return AsBool(l, n.A!, "or") | AsBool(r, n.B!, "or");
                    case Op.Eqv: return AsBool(l, n.A!, "eqv") == AsBool(r, n.B!, "eqv");
                    case Op.Lt: return AsInt(l, n.A!, "<") < AsInt(r, n.B!, "<");
                    case Op.Gt: return AsInt(l, n.A!, ">") > AsInt(r, n.B!, ">");
                    case Op.Eq:
                        if (l is bool lb && r is bool rb) return lb == rb;
                        if (l is BigInteger li && r is BigInteger ri) return li == ri;
                        throw Err("Неможливо порівняти число з логічним значенням", n, false);
                    default:
                        throw Err("Невідома операція", n, false);
                }
            }
        }
    }

    private static bool AsBool(object v, Node n, string op) =>
        v is bool b ? b : throw Err($"Операція '{op}' вимагає логічного операнда, а отримано число", n, false);

    private static BigInteger AsInt(object v, Node n, string op) =>
        v is BigInteger i ? i : throw Err($"Операція '{op}' вимагає числового операнда, а отримано логічне значення", n, false);
}
