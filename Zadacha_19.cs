using System;
using System.Collections;
using System.Collections.Generic;

// MyTreeMap<K, V> - реализация красно-черного дерева

public class MyTreeMap<K, V> : IEnumerable<KeyValuePair<K, V>>
{
    private enum Color { Red, Black }

    private class Node
    {
        public K Key;
        public V Value;
        public Node Left, Right, Parent;
        public Color Color;

        public Node(K key, V value)
        {
            Key = key;
            Value = value;
            Color = Color.Red; // новый узел всегда красный
        }
    }

    private Node root;
    private int size;
    public IComparer<K> comparator;

    // КОНСТРУКТОРЫ 

    // 1) конструктор с естественным порядком сортировки
    public MyTreeMap()
    {
        comparator = Comparer<K>.Default;
    }

    // 2) конструктор с указанным компаратором
    public MyTreeMap(IComparer<K> comp)
    {
        comparator = comp ?? Comparer<K>.Default;
    }

    // ==================== ОСНОВНЫЕ МЕТОДЫ ====================

    // 3) удаление всех пар
    public void Clear()
    {
        root = null;
        size = 0;
    }

    // 4) проверка наличия ключа
    public bool ContainsKey(object key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return GetNode((K)key) != null;
    }

    // 5) проверка наличия значения (полный обход дерева)
    public bool ContainsValue(object value)
    {
        return ContainsValue(root, value);
    }

    private bool ContainsValue(Node node, object value)
    {
        if (node == null) return false;
        if (Equals(node.Value, value)) return true;
        return ContainsValue(node.Left, value) || ContainsValue(node.Right, value);
    }

    // 6) множество всех пар
    public ISet<KeyValuePair<K, V>> EntrySet()
    {
        HashSet<KeyValuePair<K, V>> set = new HashSet<KeyValuePair<K, V>>();
        foreach (var pair in this) set.Add(pair);
        return set;
    }

    // 7) получение значения по ключу
    public V Get(object key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Node node = GetNode((K)key);
        return node != null ? node.Value : default(V);
    }

    private Node GetNode(K key)
    {
        Node current = root;
        while (current != null)
        {
            int cmp = comparator.Compare(key, current.Key);
            if (cmp < 0) current = current.Left;
            else if (cmp > 0) current = current.Right;
            else return current;
        }
        return null;
    }

    // 8) проверка на пустоту
    public bool IsEmpty() => size == 0;

    // 9) множество всех ключей
    public ISet<K> KeySet()
    {
        HashSet<K> set = new HashSet<K>();
        foreach (var pair in this) set.Add(pair.Key);
        return set;
    }

    // 10) добавление пары
    public void Put(K key, V value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        if (root == null)
        {
            root = new Node(key, value);
            root.Color = Color.Black;
            size++;
            return;
        }

        Node parent = null;
        Node current = root;
        int cmp = 0;

        while (current != null)
        {
            parent = current;
            cmp = comparator.Compare(key, current.Key);
            if (cmp < 0) current = current.Left;
            else if (cmp > 0) current = current.Right;
            else
            {
                current.Value = value; // обновление существующего ключа
                return;
            }
        }

        Node newNode = new Node(key, value);
        newNode.Parent = parent;

        if (cmp < 0) parent.Left = newNode;
        else parent.Right = newNode;

        size++;
        FixInsert(newNode); // восстановление свойств RB-дерева
    }

    // 11) удаление по ключу
    public V Remove(object key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Node node = GetNode((K)key);
        if (node == null) return default(V);
        V oldValue = node.Value;
        DeleteNode(node);
        return oldValue;
    }

    // 12) количество элементов
    public int Size() => size;

    // восстановление после вставки (перекрашивание и повороты)
    private void FixInsert(Node node)
    {
        while (node != root && node.Parent.Color == Color.Red)
        {
            Node parent = node.Parent;
            Node grandparent = parent.Parent;

            if (parent == grandparent.Left)
            {
                Node uncle = grandparent.Right;
                if (uncle != null && uncle.Color == Color.Red)
                {
                    // случай 1: дядя красный → перекрашивание
                    parent.Color = Color.Black;
                    uncle.Color = Color.Black;
                    grandparent.Color = Color.Red;
                    node = grandparent;
                }
                else
                {
                    if (node == parent.Right)
                    {
                        // случай 2: узел справа → левый поворот
                        node = parent;
                        RotateLeft(node);
                        parent = node.Parent;
                        grandparent = parent.Parent;
                    }
                    // случай 3: левый поворот и перекрашивание
                    parent.Color = Color.Black;
                    grandparent.Color = Color.Red;
                    RotateRight(grandparent);
                }
            }
            else // симметрично для правого родителя
            {
                Node uncle = grandparent.Left;
                if (uncle != null && uncle.Color == Color.Red)
                {
                    parent.Color = Color.Black;
                    uncle.Color = Color.Black;
                    grandparent.Color = Color.Red;
                    node = grandparent;
                }
                else
                {
                    if (node == parent.Left)
                    {
                        node = parent;
                        RotateRight(node);
                        parent = node.Parent;
                        grandparent = parent.Parent;
                    }
                    parent.Color = Color.Black;
                    grandparent.Color = Color.Red;
                    RotateLeft(grandparent);
                }
            }
        }
        root.Color = Color.Black;
    }

    // левый поворот: правый потомок становится родителем
    private void RotateLeft(Node x)
    {
        Node y = x.Right;
        x.Right = y.Left;
        if (y.Left != null) y.Left.Parent = x;
        y.Parent = x.Parent;
        if (x.Parent == null) root = y;
        else if (x == x.Parent.Left) x.Parent.Left = y;
        else x.Parent.Right = y;
        y.Left = x;
        x.Parent = y;
    }

    // правый поворот: левый потомок становится родителем
    private void RotateRight(Node x)
    {
        Node y = x.Left;
        x.Left = y.Right;
        if (y.Right != null) y.Right.Parent = x;
        y.Parent = x.Parent;
        if (x.Parent == null) root = y;
        else if (x == x.Parent.Right) x.Parent.Right = y;
        else x.Parent.Left = y;
        y.Right = x;
        x.Parent = y;
    }

    private void DeleteNode(Node node)
    {
        Node y = node;
        Node x;
        Color yOriginalColor = y.Color;

        if (node.Left == null)
        {
            x = node.Right;
            Transplant(node, node.Right);
        }
        else if (node.Right == null)
        {
            x = node.Left;
            Transplant(node, node.Left);
        }
        else
        {
            y = Minimum(node.Right);
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == node)
            {
                if (x != null) x.Parent = y;
            }
            else
            {
                Transplant(y, y.Right);
                y.Right = node.Right;
                y.Right.Parent = y;
            }
            Transplant(node, y);
            y.Left = node.Left;
            y.Left.Parent = y;
            y.Color = node.Color;
        }

        if (yOriginalColor == Color.Black && x != null)
            FixDelete(x);
        size--;
    }

    // восстановление после удаления
    private void FixDelete(Node node)
    {
        while (node != root && GetColor(node) == Color.Black)
        {
            if (node == node.Parent.Left)
            {
                Node sibling = node.Parent.Right;
                if (GetColor(sibling) == Color.Red)
                {
                    sibling.Color = Color.Black;
                    node.Parent.Color = Color.Red;
                    RotateLeft(node.Parent);
                    sibling = node.Parent.Right;
                }
                if (GetColor(sibling.Left) == Color.Black && GetColor(sibling.Right) == Color.Black)
                {
                    sibling.Color = Color.Red;
                    node = node.Parent;
                }
                else
                {
                    if (GetColor(sibling.Right) == Color.Black)
                    {
                        sibling.Left.Color = Color.Black;
                        sibling.Color = Color.Red;
                        RotateRight(sibling);
                        sibling = node.Parent.Right;
                    }
                    sibling.Color = node.Parent.Color;
                    node.Parent.Color = Color.Black;
                    sibling.Right.Color = Color.Black;
                    RotateLeft(node.Parent);
                    node = root;
                }
            }
            else
            {
                Node sibling = node.Parent.Left;
                if (GetColor(sibling) == Color.Red)
                {
                    sibling.Color = Color.Black;
                    node.Parent.Color = Color.Red;
                    RotateRight(node.Parent);
                    sibling = node.Parent.Left;
                }
                if (GetColor(sibling.Right) == Color.Black && GetColor(sibling.Left) == Color.Black)
                {
                    sibling.Color = Color.Red;
                    node = node.Parent;
                }
                else
                {
                    if (GetColor(sibling.Left) == Color.Black)
                    {
                        sibling.Right.Color = Color.Black;
                        sibling.Color = Color.Red;
                        RotateLeft(sibling);
                        sibling = node.Parent.Left;
                    }
                    sibling.Color = node.Parent.Color;
                    node.Parent.Color = Color.Black;
                    sibling.Left.Color = Color.Black;
                    RotateRight(node.Parent);
                    node = root;
                }
            }
        }
        if (node != null) node.Color = Color.Black;
    }

    private Color GetColor(Node node) => node == null ? Color.Black : node.Color;

    private void Transplant(Node u, Node v)
    {
        if (u.Parent == null) root = v;
        else if (u == u.Parent.Left) u.Parent.Left = v;
        else u.Parent.Right = v;
        if (v != null) v.Parent = u.Parent;
    }

    private Node Minimum(Node node)
    {
        while (node.Left != null) node = node.Left;
        return node;
    }

    private Node Maximum(Node node)
    {
        while (node.Right != null) node = node.Right;
        return node;
    }

    // НАВИГАЦИОННЫЕ МЕТОДЫ 

    // 13) наименьший ключ
    public K FirstKey()
    {
        if (root == null) throw new InvalidOperationException("Map is empty");
        return Minimum(root).Key;
    }

    // 14) наибольший ключ
    public K LastKey()
    {
        if (root == null) throw new InvalidOperationException("Map is empty");
        return Maximum(root).Key;
    }

    // 15) отображение с ключами меньше end
    public MyTreeMap<K, V> HeadMap(K end)
    {
        MyTreeMap<K, V> sub = new MyTreeMap<K, V>(comparator);
        AddSubMap(root, end, true, sub);
        return sub;
    }

    // 16) отображение с ключами от start до end
    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        MyTreeMap<K, V> sub = new MyTreeMap<K, V>(comparator);
        AddSubMapRange(root, start, end, sub);
        return sub;
    }

    // 17) отображение с ключами больше start
    public MyTreeMap<K, V> TailMap(K start)
    {
        MyTreeMap<K, V> sub = new MyTreeMap<K, V>(comparator);
        AddSubMap(root, start, false, sub);
        return sub;
    }

    private void AddSubMap(Node node, K bound, bool lessThan, MyTreeMap<K, V> result)
    {
        if (node == null) return;
        int cmp = comparator.Compare(node.Key, bound);
        if (lessThan && cmp < 0 || !lessThan && cmp >= 0)
        {
            result.Put(node.Key, node.Value);
        }
        AddSubMap(node.Left, bound, lessThan, result);
        AddSubMap(node.Right, bound, lessThan, result);
    }

    private void AddSubMapRange(Node node, K start, K end, MyTreeMap<K, V> result)
    {
        if (node == null) return;
        if (comparator.Compare(node.Key, start) >= 0 && comparator.Compare(node.Key, end) < 0)
        {
            result.Put(node.Key, node.Value);
        }
        AddSubMapRange(node.Left, start, end, result);
        AddSubMapRange(node.Right, start, end, result);
    }

    // ENTRY МЕТОДЫ 

    // 18) пара с ключом меньше заданного
    public KeyValuePair<K, V>? LowerEntry(K key)
    {
        Node node = FindLower(key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }

    private Node FindLower(K key)
    {
        Node current = root;
        Node result = null;
        while (current != null)
        {
            int cmp = comparator.Compare(current.Key, key);
            if (cmp < 0)
            {
                result = current;
                current = current.Right;
            }
            else current = current.Left;
        }
        return result;
    }

    // 19) пара с ключом меньше или равным заданному
    public KeyValuePair<K, V>? FloorEntry(K key)
    {
        Node node = FindFloor(key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }

    private Node FindFloor(K key)
    {
        Node current = root;
        Node result = null;
        while (current != null)
        {
            int cmp = comparator.Compare(current.Key, key);
            if (cmp <= 0)
            {
                result = current;
                current = current.Right;
            }
            else current = current.Left;
        }
        return result;
    }

    // 20) пара с ключом больше заданного
    public KeyValuePair<K, V>? HigherEntry(K key)
    {
        Node node = FindHigher(key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }

    private Node FindHigher(K key)
    {
        Node current = root;
        Node result = null;
        while (current != null)
        {
            int cmp = comparator.Compare(current.Key, key);
            if (cmp > 0)
            {
                result = current;
                current = current.Left;
            }
            else current = current.Right;
        }
        return result;
    }

    // 21) пара с ключом больше или равным заданному
    public KeyValuePair<K, V>? CeilingEntry(K key)
    {
        Node node = FindCeiling(key);
        return node != null ? new KeyValuePair<K, V>(node.Key, node.Value) : (KeyValuePair<K, V>?)null;
    }

    private Node FindCeiling(K key)
    {
        Node current = root;
        Node result = null;
        while (current != null)
        {
            int cmp = comparator.Compare(current.Key, key);
            if (cmp >= 0)
            {
                result = current;
                current = current.Left;
            }
            else current = current.Right;
        }
        return result;
    }

    // KEY МЕТОДЫ 

    // 22) ключ, который меньше заданного
    public K LowerKey(K key)
    {
        Node node = FindLower(key);
        if (node == null) throw new InvalidOperationException("No lower key");
        return node.Key;
    }

    // 23) ключ, который меньше или равен заданному
    public K FloorKey(K key)
    {
        Node node = FindFloor(key);
        if (node == null) throw new InvalidOperationException("No floor key");
        return node.Key;
    }

    // 24) ключ, который больше заданного
    public K HigherKey(K key)
    {
        Node node = FindHigher(key);
        if (node == null) throw new InvalidOperationException("No higher key");
        return node.Key;
    }

    // 25) ключ, который больше или равен заданному
    public K CeilingKey(K key)
    {
        Node node = FindCeiling(key);
        if (node == null) throw new InvalidOperationException("No ceiling key");
        return node.Key;
    }

    // POLL МЕТОДЫ 

    // 26) удаление и возврат первого элемента
    public KeyValuePair<K, V>? PollFirstEntry()
    {
        if (root == null) return null;
        Node node = Minimum(root);
        KeyValuePair<K, V> entry = new KeyValuePair<K, V>(node.Key, node.Value);
        DeleteNode(node);
        return entry;
    }

    // 27) удаление и возврат последнего элемента
    public KeyValuePair<K, V>? PollLastEntry()
    {
        if (root == null) return null;
        Node node = Maximum(root);
        KeyValuePair<K, V> entry = new KeyValuePair<K, V>(node.Key, node.Value);
        DeleteNode(node);
        return entry;
    }

    // 28) первый элемент без удаления
    public KeyValuePair<K, V>? FirstEntry()
    {
        if (root == null) return null;
        Node node = Minimum(root);
        return new KeyValuePair<K, V>(node.Key, node.Value);
    }

    // 29) последний элемент без удаления
    public KeyValuePair<K, V>? LastEntry()
    {
        if (root == null) return null;
        Node node = Maximum(root);
        return new KeyValuePair<K, V>(node.Key, node.Value);
    }

    //  ИТЕРАТОР 

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        return InOrderTraversal().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // обход в порядке возрастания (in-order)
    private IEnumerable<KeyValuePair<K, V>> InOrderTraversal()
    {
        Stack<Node> stack = new Stack<Node>();
        Node current = root;
        while (current != null || stack.Count > 0)
        {
            while (current != null)
            {
                stack.Push(current);
                current = current.Left;
            }
            current = stack.Pop();
            yield return new KeyValuePair<K, V>(current.Key, current.Value);
            current = current.Right;
        }
    }
}

// MyTreeSet<E> - множество на основе красно-черного дерева

public class MyTreeSet<E> : IEnumerable<E>
{
    // 1) поле m – отображение для хранения элементов
    private MyTreeMap<E, object> map;
    private static readonly object PRESENT = new object(); // фиктивное значение

    // ==================== КОНСТРУКТОРЫ ====================

    // 1) пустое множество с естественным порядком
    public MyTreeSet()
    {
        map = new MyTreeMap<E, object>();
    }

    // 2) множество с готовым отображением
    public MyTreeSet(MyTreeMap<E, object> m)
    {
        map = m ?? throw new ArgumentNullException(nameof(m));
    }

    // 3) пустое множество с компаратором
    public MyTreeSet(IComparer<E> comparator)
    {
        map = new MyTreeMap<E, object>(comparator);
    }

    // 4) множество из массива
    public MyTreeSet(E[] a)
    {
        map = new MyTreeMap<E, object>();
        if (a != null) AddAll(a);
    }

    // 5) множество из сортированного множества
    public MyTreeSet(ISet<E> s)
    {
        map = new MyTreeMap<E, object>();
        if (s != null)
        {
            foreach (var item in s) Add(item);
        }
    }

    //  ОСНОВНЫЕ МЕТОДЫ 

    // 6) добавление элемента
    public bool Add(E e)
    {
        if (map.ContainsKey(e)) return false;
        map.Put(e, PRESENT);
        return true;
    }

    // 7) добавление массива
    public void AddAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        foreach (var e in a) Add(e);
    }

    // 8) очистка
    public void Clear()
    {
        map.Clear();
    }

    // 9) проверка наличия элемента
    public bool Contains(object o)
    {
        return map.ContainsKey(o);
    }

    // 10) проверка наличия всех элементов массива
    public bool ContainsAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        foreach (var e in a)
        {
            if (!Contains(e)) return false;
        }
        return true;
    }

    // 11) проверка на пустоту
    public bool IsEmpty()
    {
        return map.IsEmpty();
    }

    // 12) удаление элемента
    public bool Remove(object o)
    {
        if (!map.ContainsKey(o)) return false;
        map.Remove(o);
        return true;
    }

    // 13) удаление всех элементов массива
    public void RemoveAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        foreach (var e in a) Remove(e);
    }

    // 14) оставить только элементы массива
    public void RetainAll(E[] a)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        HashSet<E> toKeep = new HashSet<E>(a);
        List<E> toRemove = new List<E>();
        foreach (var item in this)
        {
            if (!toKeep.Contains(item)) toRemove.Add(item);
        }
        foreach (var item in toRemove) Remove(item);
    }

    // 15) размер множества
    public int Size()
    {
        return map.Size();
    }

    // 16) массив элементов
    public E[] ToArray()
    {
        E[] result = new E[Size()];
        int index = 0;
        foreach (var item in this) result[index++] = item;
        return result;
    }

    // 17) массив элементов (с переданным массивом)
    public E[] ToArray(E[] a)
    {
        if (a == null)
        {
            return ToArray();
        }
        if (a.Length < Size())
        {
            return ToArray();
        }
        int index = 0;
        foreach (var item in this) a[index++] = item;
        return a;
    }

    //  НАВИГАЦИОННЫЕ МЕТОДЫ 

    // 18) первый (наименьший) элемент
    public E First()
    {
        if (IsEmpty()) throw new InvalidOperationException("Set is empty");
        return map.FirstKey();
    }

    // 19) последний (наибольший) элемент
    public E Last()
    {
        if (IsEmpty()) throw new InvalidOperationException("Set is empty");
        return map.LastKey();
    }

    // 20) подмножество от from до to
    public MyTreeSet<E> SubSet(E fromElement, E toElement)
    {
        MyTreeSet<E> result = new MyTreeSet<E>(map.comparator);
        var subMap = map.SubMap(fromElement, toElement);
        foreach (var pair in subMap) result.Add(pair.Key);
        return result;
    }

    // 21) элементы меньше toElement
    public MyTreeSet<E> HeadSet(E toElement)
    {
        MyTreeSet<E> result = new MyTreeSet<E>(map.comparator);
        var headMap = map.HeadMap(toElement);
        foreach (var pair in headMap) result.Add(pair.Key);
        return result;
    }

    // 22) элементы больше или равные fromElement
    public MyTreeSet<E> TailSet(E fromElement)
    {
        MyTreeSet<E> result = new MyTreeSet<E>(map.comparator);
        var tailMap = map.TailMap(fromElement);
        foreach (var pair in tailMap) result.Add(pair.Key);
        return result;
    }

    // ПОИСК ГРАНИЧНЫХ ЭЛЕМЕНТОВ 

    // 23) наименьший элемент >= obj
    public E Ceiling(E obj)
    {
        var entry = map.CeilingEntry(obj);
        return entry.HasValue ? entry.Value.Key : default(E);
    }

    // 24) наибольший элемент <= obj
    public E Floor(E obj)
    {
        var entry = map.FloorEntry(obj);
        return entry.HasValue ? entry.Value.Key : default(E);
    }

    // 25) наименьший элемент > obj
    public E Higher(E obj)
    {
        var entry = map.HigherEntry(obj);
        return entry.HasValue ? entry.Value.Key : default(E);
    }

    // 26) наибольший элемент < obj
    public E Lower(E obj)
    {
        var entry = map.LowerEntry(obj);
        return entry.HasValue ? entry.Value.Key : default(E);
    }

    //  РАСШИРЕННЫЕ МЕТОДЫ С ПАРАМЕТРАМИ ВКЛЮЧЕНИЯ 

    // 27) элементы меньше upperBound (с возможным включением границы)
    public MyTreeSet<E> HeadSet(E upperBound, bool incl)
    {
        MyTreeSet<E> result = new MyTreeSet<E>(map.comparator);
        foreach (var item in this)
        {
            int cmp = ((IComparer<E>)map.comparator).Compare(item, upperBound);
            if (incl ? cmp <= 0 : cmp < 0) result.Add(item);
        }
        return result;
    }

    // 28) элементы в диапазоне с указанием включения границ
    public MyTreeSet<E> SubSet(E lowerBound, bool lowIncl, E upperBound, bool highIncl)
    {
        MyTreeSet<E> result = new MyTreeSet<E>(map.comparator);
        foreach (var item in this)
        {
            int cmpLow = ((IComparer<E>)map.comparator).Compare(item, lowerBound);
            int cmpHigh = ((IComparer<E>)map.comparator).Compare(item, upperBound);
            bool lowOk = lowIncl ? cmpLow >= 0 : cmpLow > 0;
            bool highOk = highIncl ? cmpHigh <= 0 : cmpHigh < 0;
            if (lowOk && highOk) result.Add(item);
        }
        return result;
    }

    // 29) элементы больше fromElement (с возможным включением границы)
    public MyTreeSet<E> TailSet(E fromElement, bool inclusive)
    {
        MyTreeSet<E> result = new MyTreeSet<E>(map.comparator);
        foreach (var item in this)
        {
            int cmp = ((IComparer<E>)map.comparator).Compare(item, fromElement);
            if (inclusive ? cmp >= 0 : cmp > 0) result.Add(item);
        }
        return result;
    }

    // POLL МЕТОДЫ 

    // 30) удаление и возврат первого элемента
    public E PollFirst()
    {
        if (IsEmpty()) return default(E);
        var entry = map.PollFirstEntry();
        return entry.HasValue ? entry.Value.Key : default(E);
    }

    // 31) удаление и возврат последнего элемента
    public E PollLast()
    {
        if (IsEmpty()) return default(E);
        var entry = map.PollLastEntry();
        return entry.HasValue ? entry.Value.Key : default(E);
    }

    // ОБРАТНЫЙ ИТЕРАТОР И ОБРАТНОЕ МНОЖЕСТВО 

    // 32) итератор от большего к меньшему
    public IEnumerator<E> DescendingIterator()
    {
        List<E> list = new List<E>(this);
        for (int i = list.Count - 1; i >= 0; i--)
        {
            yield return list[i];
        }
    }

    // 33) обратное множество
    public MyTreeSet<E> DescendingSet()
    {
        MyTreeSet<E> result = new MyTreeSet<E>(map.comparator);
        List<E> list = new List<E>(this);
        list.Reverse();
        foreach (var item in list) result.Add(item);
        return result;
    }

    // ИТЕРАТОР 

    public IEnumerator<E> GetEnumerator()
    {
        foreach (var pair in map)
        {
            yield return pair.Key;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Program
{
    public static void Main()
    {
        Console.WriteLine("    Демонстрация MyTreeMap \n");

        var map = new MyTreeMap<int, string>();

        map.Put(5, "Пять");
        map.Put(3, "Три");
        map.Put(7, "Семь");
        map.Put(1, "Один");
        map.Put(9, "Девять");
        map.Put(4, "Четыре");

        Console.WriteLine("Размер карты: " + map.Size());
        Console.WriteLine("Содержит ключ 3: " + map.ContainsKey(3));
        Console.WriteLine("Значение по ключу 5: " + map.Get(5));
        Console.WriteLine("Первый ключ: " + map.FirstKey());
        Console.WriteLine("Последний ключ: " + map.LastKey());

        Console.WriteLine("\nВсе записи:");
        foreach (var entry in map)
        {
            Console.WriteLine($"  {entry.Key} -> {entry.Value}");
        }

        Console.WriteLine("\n    Демонстрация MyTreeSet \n");

        var set = new MyTreeSet<int>();
        int[] numbers = { 5, 3, 7, 1, 9, 4, 5, 2 };

        foreach (var num in numbers)
        {
            set.Add(num);
        }

        Console.WriteLine("Размер множества: " + set.Size());
        Console.WriteLine("Содержит 3: " + set.Contains(3));
        Console.WriteLine("Содержит 6: " + set.Contains(6));
        Console.WriteLine("Первый элемент: " + set.First());
        Console.WriteLine("Последний элемент: " + set.Last());

        Console.WriteLine("\nВсе элементы в отсортированном порядке:");
        foreach (var item in set)
        {
            Console.Write(item + " ");
        }
        Console.WriteLine();

        Console.WriteLine("\nНавигация:");
        Console.WriteLine($"Потолок для 4 (минимальный >= 4): {set.Ceiling(4)}");
        Console.WriteLine($"Пол для 4 (максимальный <= 4): {set.Floor(4)}");
        Console.WriteLine($"Строго больше 5: {set.Higher(5)}");
        Console.WriteLine($"Строго меньше 5: {set.Lower(5)}");

        Console.WriteLine("\nПодмножество от 2 до 7:");
        var subSet = set.SubSet(2, 7);
        foreach (var item in subSet)
        {
            Console.Write(item + " ");
        }
        Console.WriteLine();

        Console.WriteLine("\nУдаление 3: " + set.Remove(3));
        Console.WriteLine("Размер множества после удаления: " + set.Size());
        Console.WriteLine("Содержит 3: " + set.Contains(3));

        Console.WriteLine("\nВсе элементы после удаления:");
        foreach (var item in set)
        {
            Console.Write(item + " ");
        }
        Console.WriteLine();
    }
}
