namespace Hell;

internal abstract class Shape {
    internal abstract ulong Area();
}

internal sealed class Square : Shape {
    internal ulong Side;

    internal override ulong Area() {
        return Side * Side;
    }
}

internal sealed class Rect : Shape {
    internal ulong W;
    internal ulong H;

    internal override ulong Area() {
        return W * H;
    }
}

internal sealed class Node {
    internal ulong Value;
    internal Node? Next;
}

internal static class HeapTest {
    internal static void Run() {
        var empty = new Node();
        if (empty.Value != 0 || empty.Next != null) {
            Panic.WithMessage("heap: object not zeroed");
        }

        Node? head = null;
        for (var i = 0UL; i < 1000; i++) {
            head = new Node { Value = i, Next = head };
        }

        var sum = 0UL;
        for (var n = head; n != null; n = n.Next) {
            sum += n.Value;
        }

        Console.Write("list sum: ");
        Console.Write(sum);
        Console.WriteLine();

        var squares = new ulong[1000];
        for (var i = 0; i < squares.Length; i++) {
            squares[i] = (ulong)(i * i);
        }

        var squareSum = 0UL;
        for (var i = 0; i < squares.Length; i++) {
            squareSum += squares[i];
        }

        Console.Write("array sum: ");
        Console.Write(squareSum);
        Console.WriteLine();

        Shape a = new Square { Side = 7 };
        Shape b = new Rect { W = 3, H = 5 };
        Console.Write("areas: ");
        Console.Write(a.Area());
        Console.Write(" ");
        Console.Write(b.Area());
        Console.WriteLine();
    }
}