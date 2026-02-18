using System;
using System.Collections.Generic;

//Интерфейс - пустая обертка над IComparer
public interface TreeMapComparator<T> : IComparer<T> { }

//Класс для узла дерева
public class TreeNode<K, V>
{
    public K Key;
    public V Value;
    public TreeNode<K, V> Left;
    public TreeNode<K, V> Right;
    public TreeNode<K, V> Parent;

    public TreeNode(K key, V value)
    {
        Key = key;
        Value = value;
        Left = null;
        Right = null;
        Parent = null;
    }
}

//Реализация пары "ключ-значение"
public class MapEntry<K, V>
{
    public K Key { get; set; }
    public V Value { get; set; }

    public MapEntry(K key, V value)
    {
        Key = key;
        Value = value;
    }

    public override bool Equals(object obj)
    {
        if (obj is MapEntry<K, V> other)
            return EqualityComparer<K>.Default.Equals(Key, other.Key);
        return false;
    }

    public override int GetHashCode()
    {
        return Key == null ? 0 : Key.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Key} = {Value}";
    }
}

//Основной класс MyTreeMap
public class MyTreeMap<K, V>
{
    private TreeMapComparator<K> comparator;  
    private TreeNode<K, V> root;
    private int size;

    //Конструктор с естественным порядком
    public MyTreeMap()
    {
        //Создаем компаратор, реализующий интерфейс TreeMapComparator
        comparator = new NaturalOrderComparator<K>();
        root = null;
        size = 0;
        Console.WriteLine("  [КОНСТРУКТОР] Создано пустое дерево");
    }

    //Конструктор с компаратором (используем интерфейс из задания)
    public MyTreeMap(TreeMapComparator<K> comp)
    {
        comparator = comp ?? new NaturalOrderComparator<K>();
        root = null;
        size = 0;
        Console.WriteLine("  [КОНСТРУКТОР] Создано пустое дерево с компаратором");
    }

    //Вспомогательный класс для естественного порядка
    private class NaturalOrderComparator<T> : TreeMapComparator<T>
    {
        public int Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y);
        }
    }


    public void Clear()
    {
        root = null;
        size = 0;
        Console.WriteLine("  [CLEAR] Дерево очищено");
    }

    public bool ContainsKey(K key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        bool result = GetNode(key) != null;
        Console.WriteLine($"  [CONTAINS KEY] Ключ '{key}' {(result ? "найден" : "не найден")}");
        return result;
    }

    public bool ContainsValue(V value)
    {
        bool result = ContainsValueRecursive(root, value);
        Console.WriteLine($"  [CONTAINS VALUE] Значение '{value}' {(result ? "найдено" : "не найдено")}");
        return result;
    }

    private bool ContainsValueRecursive(TreeNode<K, V> node, V value)
    {
        if (node == null) return false;
        if (EqualityComparer<V>.Default.Equals(node.Value, value)) return true;
        return ContainsValueRecursive(node.Left, value) || ContainsValueRecursive(node.Right, value);
    }

    public HashSet<MapEntry<K, V>> EntrySet()
    {
        var set = new HashSet<MapEntry<K, V>>();
        InOrderEntrySet(root, set);
        Console.WriteLine($"  [ENTRY SET] Получено {set.Count} пар");
        return set;
    }
    //Вспомогательный метод, обходит дерево и собирает пары в HashSet
    private void InOrderEntrySet(TreeNode<K, V> node, HashSet<MapEntry<K, V>> set)
    {
        if (node == null) return;
        InOrderEntrySet(node.Left, set);
        set.Add(new MapEntry<K, V>(node.Key, node.Value));
        InOrderEntrySet(node.Right, set);
    }

    public V Get(K key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        var node = GetNode(key);
        V result = node != null ? node.Value : default;
        Console.WriteLine($"  [GET] По ключу '{key}' получено: {(result == null ? "null" : result.ToString())}");
        return result;
    }

    public bool IsEmpty()
    {
        bool result = size == 0;
        Console.WriteLine($"  [IS EMPTY] Дерево {(result ? "пусто" : "не пусто")}");
        return result;
    }
    //Выполняет симметричный обход и собирает все ключи в HashSet 
    public HashSet<K> KeySet()
    {
        var set = new HashSet<K>();
        InOrderKeySet(root, set);
        Console.WriteLine($"  [KEY SET] Получено {set.Count} ключей");
        return set;
    }

    private void InOrderKeySet(TreeNode<K, V> node, HashSet<K> set)
    {
        if (node == null) return;
        InOrderKeySet(node.Left, set);
        set.Add(node.Key);
        InOrderKeySet(node.Right, set);
    }

    public V Put(K key, V value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Console.WriteLine($"  [PUT] Добавление: {key} = {value}");

        if (root == null)
        {
            root = new TreeNode<K, V>(key, value);//созд новый узнл и делаем его корнем
            size = 1;
            Console.WriteLine($"    - Корень: {key}");
            return default;
        }

        TreeNode<K, V> current = root;
        TreeNode<K, V> parent = null;

        while (current != null)
        {
            parent = current;
            int cmp = comparator.Compare(key, current.Key);

            if (cmp == 0)//ключ уже есть
            {
                V old = current.Value;
                current.Value = value;
                Console.WriteLine($"    - Ключ {key} уже есть, значение обновлено");
                return old;
            }
            else if (cmp < 0)
                current = current.Left;
            else
                current = current.Right;
        }

        TreeNode<K, V> newNode = new TreeNode<K, V>(key, value);//создаём новй узел
        newNode.Parent = parent;

        if (comparator.Compare(key, parent.Key) < 0)
        {
            parent.Left = newNode;
            Console.WriteLine($"    - Добавлен слева от {parent.Key}");
        }
        else
        {
            parent.Right = newNode;
            Console.WriteLine($"    - Добавлен справа от {parent.Key}");
        }

        size++;
        Console.WriteLine($"    - Размер теперь: {size}");
        return default;
    }

    public V Remove(K key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Console.WriteLine($"  [REMOVE] Удаление ключа {key}");

        var node = GetNode(key);
        if (node == null)
        {
            Console.WriteLine($"    - Ключ {key} не найден");
            return default;
        }

        V oldValue = node.Value;
        RemoveNode(node);
        size--;
        Console.WriteLine($"    - Ключ {key} удален, размер теперь: {size}");
        return oldValue;
    }
    //Удаление узла
    private void RemoveNode(TreeNode<K, V> node)
    {
        if (node.Left == null)
            Transplant(node, node.Right);
        else if (node.Right == null)
            Transplant(node, node.Left);
        else
        {
            var successor = GetMinNode(node.Right);
            if (successor.Parent != node)
            {
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                successor.Right.Parent = successor;
            }
            Transplant(node, successor);
            successor.Left = node.Left;
            successor.Left.Parent = successor;
        }
    }
    //пересадка узлов при удалении
    private void Transplant(TreeNode<K, V> u, TreeNode<K, V> v)
    {
        if (u.Parent == null)
            root = v;
        else if (u == u.Parent.Left)
            u.Parent.Left = v;
        else
            u.Parent.Right = v;

        if (v != null)
            v.Parent = u.Parent;
    }

    public int Size()
    {
        Console.WriteLine($"  [SIZE] Размер: {size}");
        return size;
    }

    public K FirstKey()
    {
        if (root == null) throw new InvalidOperationException("Дерево пусто");
        K result = GetMinNode(root).Key;
        Console.WriteLine($"  [FIRST KEY] Первый ключ: {result}");
        return result;
    }

    public K LastKey()
    {
        if (root == null) throw new InvalidOperationException("Дерево пусто");
        K result = GetMaxNode(root).Key;
        Console.WriteLine($"  [LAST KEY] Последний ключ: {result}");
        return result;
    }
    //Добавляет к созданному отображение эл-ы с ключами меньше заданного
    public MyTreeMap<K, V> HeadMap(K end)
    {
        if (end == null) throw new ArgumentNullException(nameof(end));
        Console.WriteLine($"  [HEAD MAP] Ключи < {end}");
        var result = new MyTreeMap<K, V>(comparator);
        HeadMapRecursive(root, end, result);
        Console.WriteLine($"    - Найдено {result.Size()} элементов");
        return result;
    }

    private void HeadMapRecursive(TreeNode<K, V> node, K end, MyTreeMap<K, V> result)
    {
        if (node == null) return;
        if (comparator.Compare(node.Key, end) < 0)
        {
            result.Put(node.Key, node.Value);
            HeadMapRecursive(node.Left, end, result);
            HeadMapRecursive(node.Right, end, result);
        }
        else
        {
            HeadMapRecursive(node.Left, end, result);
        }
    }

    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        if (start == null || end == null) throw new ArgumentNullException();
        if (comparator.Compare(start, end) >= 0)
            throw new ArgumentException("start должен быть меньше end");

        Console.WriteLine($"  [SUB MAP] Ключи от {start} до {end}");
        var result = new MyTreeMap<K, V>(comparator);
        SubMapRecursive(root, start, end, result);
        Console.WriteLine($"    - Найдено {result.Size()} элементов");
        return result;
    }

    private void SubMapRecursive(TreeNode<K, V> node, K start, K end, MyTreeMap<K, V> result)
    {
        if (node == null) return;

        int cmpStart = comparator.Compare(node.Key, start);
        int cmpEnd = comparator.Compare(node.Key, end);

        if (cmpStart >= 0 && cmpEnd < 0)
            result.Put(node.Key, node.Value);

        if (cmpStart > 0)
            SubMapRecursive(node.Left, start, end, result);
        if (cmpEnd < 0)
            SubMapRecursive(node.Right, start, end, result);
    }

    public MyTreeMap<K, V> TailMap(K start)
    {
        if (start == null) throw new ArgumentNullException(nameof(start));
        Console.WriteLine($"  [TAIL MAP] Ключи >= {start}");
        var result = new MyTreeMap<K, V>(comparator);
        TailMapRecursive(root, start, result);
        Console.WriteLine($"    - Найдено {result.Size()} элементов");
        return result;
    }

    private void TailMapRecursive(TreeNode<K, V> node, K start, MyTreeMap<K, V> result)
    {
        if (node == null) return;
        if (comparator.Compare(node.Key, start) >= 0)
        {
            result.Put(node.Key, node.Value);
            TailMapRecursive(node.Left, start, result);
            TailMapRecursive(node.Right, start, result);
        }
        else
        {
            TailMapRecursive(node.Right, start, result);
        }
    }

    public MapEntry<K, V> LowerEntry(K key)
    {
        var node = LowerNode(key);
        var result = node != null ? new MapEntry<K, V>(node.Key, node.Value) : null;
        Console.WriteLine($"  [LOWER ENTRY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public MapEntry<K, V> FloorEntry(K key)
    {
        var node = FloorNode(key);
        var result = node != null ? new MapEntry<K, V>(node.Key, node.Value) : null;
        Console.WriteLine($"  [FLOOR ENTRY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public MapEntry<K, V> HigherEntry(K key)
    {
        var node = HigherNode(key);
        var result = node != null ? new MapEntry<K, V>(node.Key, node.Value) : null;
        Console.WriteLine($"  [HIGHER ENTRY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public MapEntry<K, V> CeilingEntry(K key)
    {
        var node = CeilingNode(key);
        var result = node != null ? new MapEntry<K, V>(node.Key, node.Value) : null;
        Console.WriteLine($"  [CEILING ENTRY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public K LowerKey(K key)
    {
        var node = LowerNode(key);
        K result = node != null ? node.Key : default;
        Console.WriteLine($"  [LOWER KEY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public K FloorKey(K key)
    {
        var node = FloorNode(key);
        K result = node != null ? node.Key : default;
        Console.WriteLine($"  [FLOOR KEY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public K HigherKey(K key)
    {
        var node = HigherNode(key);
        K result = node != null ? node.Key : default;
        Console.WriteLine($"  [HIGHER KEY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public K CeilingKey(K key)
    {
        var node = CeilingNode(key);
        K result = node != null ? node.Key : default;
        Console.WriteLine($"  [CEILING KEY] Для {key}: {(result == null ? "нет" : result.ToString())}");
        return result;
    }

    public MapEntry<K, V> PollFirstEntry()
    {
        if (root == null)
        {
            Console.WriteLine("  [POLL FIRST] Дерево пусто");
            return null;
        }
        var min = GetMinNode(root);
        var entry = new MapEntry<K, V>(min.Key, min.Value);
        Console.WriteLine($"  [POLL FIRST] Удаляем первый: {entry}");
        Remove(min.Key);
        return entry;
    }

    public MapEntry<K, V> PollLastEntry()
    {
        if (root == null)
        {
            Console.WriteLine("  [POLL LAST] Дерево пусто");
            return null;
        }
        var max = GetMaxNode(root);
        var entry = new MapEntry<K, V>(max.Key, max.Value);
        Console.WriteLine($"  [POLL LAST] Удаляем последний: {entry}");
        Remove(max.Key);
        return entry;
    }

    public MapEntry<K, V> FirstEntry()
    {
        if (root == null)
        {
            Console.WriteLine("  [FIRST ENTRY] Дерево пусто");
            return null;
        }
        var min = GetMinNode(root);
        var result = new MapEntry<K, V>(min.Key, min.Value);
        Console.WriteLine($"  [FIRST ENTRY] Первый элемент: {result}");
        return result;
    }

    public MapEntry<K, V> LastEntry()
    {
        if (root == null)
        {
            Console.WriteLine("  [LAST ENTRY] Дерево пусто");
            return null;
        }
        var max = GetMaxNode(root);
        var result = new MapEntry<K, V>(max.Key, max.Value);
        Console.WriteLine($"  [LAST ENTRY] Последний элемент: {result}");
        return result;
    }

    // Вспомогательные методы
    private TreeNode<K, V> GetNode(K key)
    {
        var current = root;
        while (current != null)
        {
            int cmp = comparator.Compare(key, current.Key);
            if (cmp == 0) return current;
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }
    //Идет налево до упора
    private TreeNode<K, V> GetMinNode(TreeNode<K, V> node)
    {
        while (node.Left != null) node = node.Left;
        return node;
    }
    //Идет направо до упора
    private TreeNode<K, V> GetMaxNode(TreeNode<K, V> node)
    {
        while (node.Right != null) node = node.Right;
        return node;
    }
    //Ищет самый большой узел, ктр меньше указ ключа
    private TreeNode<K, V> LowerNode(K key)
    {
        var current = root;
        TreeNode<K, V> candidate = null;
        while (current != null)
        {
            if (comparator.Compare(current.Key, key) < 0)
            {
                candidate = current;
                current = current.Right;
            }
            else
            {
                current = current.Left;
            }
        }
        return candidate;
    }
    //Меньше или равен
    private TreeNode<K, V> FloorNode(K key)
    {
        var current = root;
        TreeNode<K, V> candidate = null;
        while (current != null)
        {
            int cmp = comparator.Compare(current.Key, key);
            if (cmp == 0) return current;
            if (cmp < 0)
            {
                candidate = current;
                current = current.Right;
            }
            else
            {
                current = current.Left;
            }
        }
        return candidate;
    }
    //Строго больше 
    private TreeNode<K, V> HigherNode(K key)
    {
        var current = root;
        TreeNode<K, V> candidate = null;
        while (current != null)
        {
            if (comparator.Compare(current.Key, key) > 0)
            {
                candidate = current;
                current = current.Left;
            }
            else
            {
                current = current.Right;
            }
        }
        return candidate;
    }
    //Больше или равен
    private TreeNode<K, V> CeilingNode(K key)
    {
        var current = root;
        TreeNode<K, V> candidate = null;
        while (current != null)
        {
            int cmp = comparator.Compare(current.Key, key);
            if (cmp == 0) return current;
            if (cmp > 0)
            {
                candidate = current;
                current = current.Left;
            }
            else
            {
                current = current.Right;
            }
        }
        return candidate;
    }

    public void PrintTree()
    {
        Console.WriteLine("\n--- ДЕРЕВО ---");
        if (root == null)
        {
            Console.WriteLine("пусто");
        }
        else
        {
            PrintNode(root, "", true);
        }
        Console.WriteLine("---------------\n");
    }

    private void PrintNode(TreeNode<K, V> node, string prefix, bool isLast)
    {
        Console.Write(prefix);
        Console.Write(isLast ? "└─" : "├─");
        Console.WriteLine($"{node.Key}({node.Value})");

        prefix += isLast ? "  " : "| ";

        var children = new List<TreeNode<K, V>>();
        if (node.Left != null) children.Add(node.Left);
        if (node.Right != null) children.Add(node.Right);

        for (int i = 0; i < children.Count; i++)
        {
            PrintNode(children[i], prefix, i == children.Count - 1);
        }
    }
}

// Тестирование
class Program
{
    static void Main()
    {
        Console.WriteLine("ТЕСТИРОВАНИЕ MyTreeMap\n");

        // Используем первый конструктор
        var map = new MyTreeMap<int, string>();

        // Добавление элементов
        map.Put(50, "пятьдесят");
        map.Put(30, "тридцать");
        map.Put(70, "семьдесят");
        map.Put(20, "двадцать");
        map.Put(40, "сорок");
        map.Put(60, "шестьдесят");
        map.Put(80, "восемьдесят");
        map.PrintTree();

        // Тестируем методы
        Console.WriteLine($"Первый ключ: {map.FirstKey()}");
        Console.WriteLine($"Последний ключ: {map.LastKey()}");
        Console.WriteLine($"Размер: {map.Size()}");

        var first = map.FirstEntry();
        Console.WriteLine($"Первый элемент: {first}");

        var polled = map.PollFirstEntry();
        Console.WriteLine($"Удален первый: {polled}");
        map.PrintTree();

    }
}