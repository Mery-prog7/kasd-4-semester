using System;
using System.Collections.Generic;

public class MyTreeMap<K, V>
{
    private readonly IComparer<K> comparator;//Компаратор для сравнения ключей (Поле типа интерфейс) 
    private Node root; 
    private int size;  

    //Узел дерева
    private class Node
    {
        public K key;
        public V value;
        public Node left;
        public Node right;
        public Node parent;

        public Node(K key, V value, Node parent)
        {
            this.key = key;
            this.value = value;
            this.parent = parent;
        }
    }
    // Класс для пары ключ-значение
    public class MyEntry
    {
        public K Key { get; }
        public V Value { get; }

        public MyEntry(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Key} => {Value}";
        }
    }

    // 1) Конструктор: естественный порядок
    public MyTreeMap()
    {
        comparator = Comparer<K>.Default;
        root = null;
        size = 0;
    }

    // 2) Конструктор: с компаратором
    public MyTreeMap(IComparer<K> comparator)
    {
        if (comparator == null)
        {
            throw new ArgumentNullException(nameof(comparator), "Компаратор не должен быть null.");
        }

        this.comparator = comparator;
        root = null;
        size = 0;
    }

    // 3) Очистка отображения
    public void Clear()
    {
        root = null;
        size = 0;
    }

    // 4) Проверка наличия ключа
    public bool ContainsKey(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (!(key is K typedKey))
        {
            return false;
        }

        return FindNode(typedKey) != null;
    }

    // 5) Проверка наличия значения
    public bool ContainsValue(object value)
    {
        bool found = false;
        TraverseInOrder(root, node =>//обходим все узлы
        {
            if (!found)
            {
                if (value == null && node.value == null)
                {
                    found = true;
                }
                else if (value != null && value.Equals(node.value))//сравниваем значение
                {
                    found = true;
                }
            }
        });

        return found;
    }

    // 6) Возврат множества всех пар
    public HashSet<MyEntry> EntrySet()
    {
        HashSet<MyEntry> entries = new HashSet<MyEntry>();
        TraverseInOrder(root, node => entries.Add(new MyEntry(node.key, node.value)));
        return entries;
    }

    // 7) Получение значения по ключу
    public V Get(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (!(key is K typedKey))
        {
            return default(V);
        }

        Node node = FindNode(typedKey);
        if (node == null)
        {
            return default(V);
        }

        return node.value;
    }

    // 8) Проверка на пустоту
    public bool IsEmpty()
    {
        return size == 0;
    }

    // 9) Возврат множества всех ключей
    public HashSet<K> KeySet()
    {
        HashSet<K> keys = new HashSet<K>();
        TraverseInOrder(root, node => keys.Add(node.key));
        return keys;
    }

    // 10) Добавление пары ключ-значение
    public void Put(K key, V value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (root == null) //Проверка пустоты дерева
        {
            root = new Node(key, value, null); //Создаём корневой узел
            size = 1;
            return;
        }

        Node current = root;
        Node parent = null;

        while (current != null)
        {
            parent = current;
            int comparison = CompareKeys(key, current.key);

            if (comparison < 0) //если наш ключ меньше, идём налево
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                current = current.right;
            }
            else
            {
                current.value = value;
                return;
            }
        }
        //если дошли до пустого места
        Node newNode = new Node(key, value, parent);//создаем новый узел
        if (CompareKeys(key, parent.key) < 0)
        {
            parent.left = newNode;
        }
        else
        {
            parent.right = newNode;
        }

        size++;
    }

    // 11) Удаление пары по ключу
    public V Remove(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Ключ не должен быть null.");
        }

        if (!(key is K typedKey))
        {
            return default(V);
        }

        Node node = FindNode(typedKey);//ищем узел с таким ключом
        if (node == null) //не нашли
        {
            return default(V);
        }
        //нашли
        V removedValue = node.value;
        DeleteNode(node);
        size--;
        return removedValue;
    }

    // 12) Размер отображения
    public int Size()
    {
        return size;
    }

    // 13) Первый ключ
    public K FirstKey()//самый левый
    {
        if (root == null)
        {
            throw new InvalidOperationException("Отображение пустое.");
        }

        Node node = GetFirstNode();
        return node.key;
    }

    // 14) Последний ключ
    public K LastKey()//самое правое
    {
        if (root == null)
        {
            throw new InvalidOperationException("Отображение пустое.");
        }

        Node node = GetLastNode();
        return node.key;
    }

    // 15) Отображение с ключами меньше end
    public MyTreeMap<K, V> HeadMap(K end)
    {
        if (end == null)
        {
            throw new ArgumentNullException(nameof(end), "Граница end не должна быть null.");
        }

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        TraverseInOrder(root, node =>
        {
            if (CompareKeys(node.key, end) < 0)//проверяем каждый  ключ ктр меньше end
            {
                result.Put(node.key, node.value); //добавляем в новое отображение
            }
        });
        return result;
    }

    // 16) Отображение с ключами от start до end
    public MyTreeMap<K, V> SubMap(K start, K end)
    {
        if (start == null)
        {
            throw new ArgumentNullException(nameof(start), "Граница start не должна быть null.");
        }
        if (end == null)
        {
            throw new ArgumentNullException(nameof(end), "Граница end не должна быть null.");
        }
        if (CompareKeys(start, end) > 0)
        {
            throw new ArgumentException("start должен быть меньше или равен end.");
        }

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        TraverseInOrder(root, node =>
        {
            if (CompareKeys(node.key, start) >= 0 && CompareKeys(node.key, end) < 0)
            {
                result.Put(node.key, node.value);//доб те узлы чьи ключи попадают в диапазон
            }
        });
        return result;
    }

    // 17) Отображение с ключами больше start
    public MyTreeMap<K, V> TailMap(K start)
    {
        if (start == null)
        {
            throw new ArgumentNullException(nameof(start), "Граница start не должна быть null.");
        }

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        TraverseInOrder(root, node =>
        {
            if (CompareKeys(node.key, start) > 0)
            {
                result.Put(node.key, node.value);
            }
        });
        return result;
    }

    // 18) Пара с ключом меньше заданного
    public MyEntry LowerEntry(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        //поиск узла с наибольшим ключом, меньшим заданного
        Node node = FindLowerNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    // 19) Пара с ключом меньше или равным заданному
    public MyEntry FloorEntry(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        Node node = FindFloorNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    // 20) Пара с ключом больше заданного
    public MyEntry HigherEntry(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        Node node = FindHigherNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    // 21) Пара с ключом больше или равным заданному
    public MyEntry CeilingEntry(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        Node node = FindCeilingNode(key);
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    // 22) Ключ меньше заданного
    public K LowerKey(K key)
    {
        MyEntry entry = LowerEntry(key);
        return entry == null ? default(K) : entry.Key;
    }

    // 23) Ключ меньше или равный заданному
    public K FloorKey(K key)
    {
        MyEntry entry = FloorEntry(key);
        return entry == null ? default(K) : entry.Key;
    }

    // 24) Ключ больше заданного
    public K HigherKey(K key)
    {
        MyEntry entry = HigherEntry(key);
        return entry == null ? default(K) : entry.Key;
    }

    // 25) Ключ больше или равный заданному
    public K CeilingKey(K key)
    {
        MyEntry entry = CeilingEntry(key);
        return entry == null ? default(K) : entry.Key;
    }

    // 26) Удаление и возврат первого элемента
    public MyEntry PollFirstEntry()
    {
        Node node = GetFirstNode();//нашли самый левый узел
        if (node == null) // если дерево пустое возвращаем нуль
        {
            return null;
        }
        //создаём новую запись
        MyEntry entry = new MyEntry(node.key, node.value);
        DeleteNode(node);
        size--;
        return entry;
    }

    // 27) Удаление и возврат последнего элемента
    public MyEntry PollLastEntry()
    {
        Node node = GetLastNode();
        if (node == null)
        {
            return null;
        }

        MyEntry entry = new MyEntry(node.key, node.value);
        DeleteNode(node);
        size--;
        return entry;
    }

    // 28) Первый элемент без удаления
    public MyEntry FirstEntry()
    {
        Node node = GetFirstNode();
        return node == null ? null : new MyEntry(node.key, node.value);
    }

    // 29) Последний элемент без удаления
    public MyEntry LastEntry()
    {
        Node node = GetLastNode();
        return node == null ? null : new MyEntry(node.key, node.value);
    }



    // Вспомогательные методы
    private int CompareKeys(K firstKey, K secondKey)
    {
        return comparator.Compare(firstKey, secondKey);//вызываем компаратор (-, 0, +)
    }

    //бинарный поиск узла
    private Node FindNode(K key)
    {
        Node current = root;
        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                current = current.right;
            }
            else
            {
                return current;
            }
        }
        return null;
    }

    //поиск крайних узлов
    private Node GetFirstNode()
    {
        if (root == null) return null;
        Node current = root;
        while (current.left != null)
        {
            current = current.left;
        }
        return current;
    }

    private Node GetLastNode()
    {
        if (root == null) return null;
        Node current = root;
        while (current.right != null)
        {
            current = current.right;
        }
        return current;
    }

    //Симметричный обход 
    private void TraverseInOrder(Node node, Action<Node> action)
    {
        if (node == null) return;
        TraverseInOrder(node.left, action);//рекурс обойти левое поддерево
        action(node);//выполн действие
        TraverseInOrder(node.right, action); //рек обойти правое поддерево
    }

    //Удаление узла
    private void DeleteNode(Node node)
    {
        // 1. два потомка
        if (node.left != null && node.right != null)
        {
            Node successor = node.right;//преемник
            while (successor.left != null)
            {
                successor = successor.left;//левый узел правого поддерева
            }

            node.key = successor.key;
            node.value = successor.value;
            node = successor;
        }

        // 2. у node максимум один потомок
        Node replacement = node.left ?? node.right;
        if (replacement != null)
        {
            replacement.parent = node.parent;//меняем родителя 

            if (node.parent == null)//удаляемый узел не имеет родителя
            {
                root = replacement;//корнем стал ребенок
            }
            else if (node == node.parent.left)//удаляемый узел левый ребенок своего родителя
            {
                node.parent.left = replacement;
            }
            else //правый ребенок
            {
                node.parent.right = replacement;
            }
        }
        else // 3. нет потомков (лист)
        {
            if (node.parent == null) //узел корень дерева
            {
                root = null;//дерево стало пустым
            }
            else if (node == node.parent.left) //удаляемый узел левый ребенок
            {
                node.parent.left = null;
            }
            else
            {
                node.parent.right = null;
            }
        }
    }

    // Ищет узел с самым большим ключом, меньше заданного
    private Node FindLowerNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison <= 0)//заданнный клююч >= текущ ключу
            {
                current = current.left;
            }
            else
            {
                candidate = current;
                current = current.right;
            }
        }
        return candidate;
    }
    // Возвращает узел с ключом, равным заданному,если = нет, действует как findLowerNode.
    private Node FindFloorNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                candidate = current;
                current = current.right;
            }
            else
            {
                return current;
            }
        }
        return candidate;
    }
    // Ищет узел с самым маленьким ключом, который больше заданного
    private Node FindHigherNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison < 0)
            {
                candidate = current;
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }
        return candidate;
    }
    // нашли узел с точно таким же ключом
    private Node FindCeilingNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);
            if (comparison <= 0)
            {
                candidate = current;
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }
        return candidate;
    }
}

// Пример использования
public static class Program
{
    public static void Main()
    {
        MyTreeMap<int, string> map = new MyTreeMap<int, string>();

        map.Put(10, "десять");
        map.Put(20, "двадцать");
        map.Put(30, "тридцать");
        map.Put(15, "пятнадцать");

        Console.WriteLine("Размер: " + map.Size());
        Console.WriteLine("Первый ключ: " + map.FirstKey());
        Console.WriteLine("Последний ключ: " + map.LastKey());
        Console.WriteLine("Значение для ключа 20: " + map.Get(20));

        Console.WriteLine("\nВсе записи:");
        foreach (var entry in map.EntrySet())
        {
            Console.WriteLine(entry);
        }

        Console.WriteLine("\nКлючи больше 15:");
        var tailMap = map.TailMap(15);
        foreach (var key in tailMap.KeySet())
        {
            Console.WriteLine(key);
        }
    }
}
