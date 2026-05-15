using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphTasks
{
    // Задача 8: Построение минимального остовного леса. Алгоритм Прима.
    public static class PrimsMST
    {
        public static (List<(int u, int v, int weight)> mstEdges, int totalWeight)
            FindMST(int vertices, List<(int u, int v, int weight)> edges)
        {
            // Списки смежности: для каждой вершины храним (сосед, вес)
            var adj = new List<(int neighbor, int weight)>[vertices];
            for (int i = 0; i < vertices; i++)
                adj[i] = new List<(int, int)>();

            foreach (var e in edges)
            {
                adj[e.u].Add((e.v, e.weight));
                adj[e.v].Add((e.u, e.weight));
            }

            bool[] visited = new bool[vertices];
            var mstEdges = new List<(int, int, int)>();
            int totalWeight = 0;

            // Проходим по каждой компоненте связности
            for (int start = 0; start < vertices; start++)
            {
                if (visited[start]) continue;

                // Приоритетная очередь (SortedSet) по весу, затем по вершине
                var pq = new SortedSet<(int weight, int vertex, int parent)>();
                pq.Add((0, start, -1));

                while (pq.Count > 0)
                {
                    var (weight, v, parent) = pq.Min;
                    pq.Remove(pq.Min);

                    if (visited[v]) continue;
                    visited[v] = true;

                    if (parent != -1)
                    {
                        mstEdges.Add((parent, v, weight));
                        totalWeight += weight;
                    }

                    foreach (var (neighbor, w) in adj[v])
                    {
                        if (!visited[neighbor])
                            pq.Add((w, neighbor, v));
                    }
                }
            }

            return (mstEdges, totalWeight);
        }
    }

    // Задача 11: Построение максимального потока. Алгоритм Диница.
    public class DinicMaxFlow
    {
        private class Edge
        {
            public int To { get; set; }
            public int Rev { get; set; }     // Индекс обратного ребра в списке graph[To]
            public int Capacity { get; set; }
        }

        private List<Edge>[] graph;
        private int[] level;
        private int[] iter;

        public DinicMaxFlow(int n)
        {
            graph = new List<Edge>[n];
            for (int i = 0; i < n; i++)
                graph[i] = new List<Edge>();
        }

        // Добавление ребра и обратного ребра (для остаточной сети)
        public void AddEdge(int from, int to, int capacity)
        {
            graph[from].Add(new Edge { To = to, Rev = graph[to].Count, Capacity = capacity });
            graph[to].Add(new Edge { To = from, Rev = graph[from].Count - 1, Capacity = 0 });
        }

        // BFS строит слоистую сеть
        private bool Bfs(int s, int t)
        {
            level = new int[graph.Length];
            // Инициализируем уровни значением -1 (вручную, без Array.Fill)
            for (int i = 0; i < level.Length; i++)
                level[i] = -1;

            level[s] = 0;
            var queue = new Queue<int>();
            queue.Enqueue(s);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                foreach (var e in graph[v])
                {
                    if (e.Capacity > 0 && level[e.To] < 0)
                    {
                        level[e.To] = level[v] + 1;
                        queue.Enqueue(e.To);
                    }
                }
            }
            return level[t] >= 0;
        }

        // DFS ищет блокирующий поток
        private int Dfs(int v, int t, int f)
        {
            if (v == t) return f;
            for (; iter[v] < graph[v].Count; iter[v]++)
            {
                Edge e = graph[v][iter[v]];
                if (e.Capacity > 0 && level[v] < level[e.To])
                {
                    int d = Dfs(e.To, t, Math.Min(f, e.Capacity));
                    if (d > 0)
                    {
                        e.Capacity -= d;
                        graph[e.To][e.Rev].Capacity += d;
                        return d;
                    }
                }
            }
            return 0;
        }

        public int ComputeMaxFlow(int s, int t)
        {
            int flow = 0;
            while (Bfs(s, t))
            {
                iter = new int[graph.Length];
                int f;
                while ((f = Dfs(s, t, int.MaxValue)) > 0)
                    flow += f;
            }
            return flow;
        }
    }

    // Задача 14: Построение максимальной клики. Эвристический алгоритм «слияния» клик.
    public static class MaxCliqueHeuristic
    {
        // Проверка, образует ли множество вершин клику
        private static bool IsClique(List<int> vertices, List<int>[] graph)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    if (!graph[vertices[i]].Contains(vertices[j]))
                        return false;
                }
            }
            return true;
        }

        public static List<int> FindMaxClique(List<int>[] graph)
        {
            int n = graph.Length;
            // Сортируем вершины по убыванию степени для лучшей эвристики
            var verticesOrder = Enumerable.Range(0, n)
                .OrderByDescending(v => graph[v].Count)
                .ToList();

            var allCliques = new List<List<int>>();

            // Шаг 1: Жадное построение начальных клик
            foreach (var start in verticesOrder)
            {
                var clique = new List<int> { start };
                var candidates = new HashSet<int>(graph[start]); // возможные кандидаты на добавление

                while (candidates.Count > 0)
                {
                    // Выбираем кандидата с максимальной степенью внутри candidates
                    int best = candidates
                        .OrderByDescending(v => graph[v].Count(candidates.Contains))
                        .First();

                    bool canAdd = true;
                    foreach (var c in clique)
                    {
                        if (!graph[c].Contains(best))
                        {
                            canAdd = false;
                            break;
                        }
                    }

                    if (canAdd)
                    {
                        clique.Add(best);
                        // Оставляем только тех кандидатов, которые соединены с новой вершиной
                        candidates.IntersectWith(graph[best]);
                    }
                    else
                    {
                        candidates.Remove(best);
                    }
                }
                allCliques.Add(clique);
            }

            // Шаг 2: Слияние клик (эвристика)
            bool merged;
            do
            {
                merged = false;
                for (int i = 0; i < allCliques.Count; i++)
                {
                    for (int j = i + 1; j < allCliques.Count; j++)
                    {
                        var union = new HashSet<int>(allCliques[i]);
                        union.UnionWith(allCliques[j]);
                        var unionList = union.ToList();

                        if (IsClique(unionList, graph) && unionList.Count > allCliques[i].Count && unionList.Count > allCliques[j].Count)
                        {
                            allCliques[i] = unionList;
                            allCliques.RemoveAt(j);
                            merged = true;
                            break; // перезапускаем слияния после изменения
                        }
                    }
                    if (merged) break;
                }
            } while (merged);

            // Возвращаем самую большую клику
            return allCliques.OrderByDescending(c => c.Count).First();
        }
    }

    // Пример использования
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Задача 8: Алгоритм Прима (минимальный остовный лес) ===\n");

            // Граф для Прима (вершины 0..3)
            var edgesPrim = new List<(int u, int v, int weight)>
            {
                (0, 1, 4),
                (0, 2, 3),
                (1, 2, 1),
                (1, 3, 2),
                (2, 3, 5)
            };

            var (mst, totalWeight) = PrimsMST.FindMST(4, edgesPrim);
            Console.WriteLine($"Минимальный вес остовного леса: {totalWeight}");
            Console.WriteLine("Рёбра в MST:");
            foreach (var e in mst)
                Console.WriteLine($"  {e.u} -- {e.v} (вес {e.weight})");

            Console.WriteLine("\n=== Задача 11: Алгоритм Диница (максимальный поток) ===\n");

            // Транспортная сеть: вершина 0 -> ... -> 3
            var dinic = new DinicMaxFlow(4);
            dinic.AddEdge(0, 1, 3);
            dinic.AddEdge(0, 2, 2);
            dinic.AddEdge(1, 2, 1);
            dinic.AddEdge(1, 3, 2);
            dinic.AddEdge(2, 3, 3);

            int maxFlow = dinic.ComputeMaxFlow(0, 3);
            Console.WriteLine($"Максимальный поток из вершины 0 в вершину 3: {maxFlow}");

            Console.WriteLine("\n=== Задача 14: Эвристический алгоритм слияния клик ===\n");

            // Граф для поиска максимальной клики (списки смежности)
            var graphClique = new List<int>[]
            {
                new List<int> { 1, 2 },         
                new List<int> { 0, 2, 3 },       
                new List<int> { 0, 1, 3 },       
                new List<int> { 1, 2 }          
            };

            var maxClique = MaxCliqueHeuristic.FindMaxClique(graphClique);
            Console.WriteLine($"Найденная максимальная клика: {{ {string.Join(", ", maxClique)} }}");

            // Дополнительная проверка: покажем, что все вершины клики соединены
            bool cliqueOk = true;
            for (int i = 0; i < maxClique.Count; i++)
                for (int j = i + 1; j < maxClique.Count; j++)
                    if (!graphClique[maxClique[i]].Contains(maxClique[j]))
                        cliqueOk = false;
            Console.WriteLine($"Клика корректна: {cliqueOk}");

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
