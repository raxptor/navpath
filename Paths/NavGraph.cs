using System;
using System.Collections.Generic;

namespace Paths
{
	public class NavGraph
	{
		public struct Point
		{
			public float x, y;
		}

		public class Polygon
		{
			public int[] indices;
			public bool junked;
		}

		struct Edge
		{
			public int a;
			public int b;
		}

		public Point[] m_pts;
		public Polygon[] m_polys;

		public NavGraph()
		{

		}

		public static bool InTriangle(Point s, Point a, Point b, Point c)
		{
			float as_x = s.x-a.x;
			float as_y = s.y-a.y;
			bool s_ab = (b.x-a.x)*as_y-(b.y-a.y)*as_x > 0;
			if((c.x-a.x)*as_y-(c.y-a.y)*as_x > 0 == s_ab) 
				return false;
			if((c.x-b.x)*(s.y-b.y)-(c.y-b.y)*(s.x-b.x) > 0 != s_ab) 
				return false;
			return true;
		}
			

		static Random _rand = new Random(0);

		public static void RandWalk(bool[] grid, int px, int py, int w, int h, int lim)
		{
			if (px < 0 || py < 0)
				return;
			if (px >= w || py >= h)
				return;
				
			grid[w * py + px] = true;
			if (lim == 0)
				return;
				
			int num = 1;
			if (_rand.NextDouble() < 0.75)
				num = 2;

			for (int i = 0; i < num; i++)
			{
				int which = _rand.Next() % 6;
				
				if (which == 0)
					RandWalk(grid, px - 1, py, w, h, lim - 1);
				if (which == 1)
					RandWalk(grid, px + 1, py, w, h, lim - 1);
				if (which == 2)
					RandWalk(grid, px, py - 1, w, h, lim - 1);
				if (which == 3)
					RandWalk(grid, px, py + 1, w, h, lim - 1);
			}
		}


		public static int EdgeIndex(int a, int b, int count)
		{
			if (a < b)
				return a * count + b;
			else
				return b * count + a;
		}

		public static bool IsConvex(List<Point> pts, List<int> indices)
		{
			float nx=0, ny=0;
			for (int i = 0; i <= indices.Count; i++)
			{
				int k0 = indices[i % indices.Count];
				int k1 = indices[(i + 1) % indices.Count];
				float dx = pts[k1].x - pts[k0].x;
				float dy = pts[k1].y - pts[k0].y;
				if (i > 0)
				{
					if (dx * nx + dy * ny < 0)
						return false;
				}
				nx = -dy;
				ny = dx;
			}
			return true;
		}

		public static void Optimize(List<Point> pts, List<int> indices)
		{
			float nx=0, ny=0;
			for (int i = 0; i <= indices.Count; i++)
			{
				int k0 = indices[i % indices.Count];
				int k1 = indices[(i + 1) % indices.Count];
				float dx = pts[k1].x - pts[k0].x;
				float dy = pts[k1].y - pts[k0].y;
				if (i > 0)
				{
					if (dx * nx + dy * ny == 0)
					{
						pts.RemoveAt(k0);
						Optimize(pts, indices);
						return;
					}
				}
				nx = -dy;
				ny = dx;
			}
		}

		public static NavGraph MakeRandom(Random r)
		{
			// grid size
			int gw = 32;
			int gh = 30;
			float scalex = 30;
			float scaley = 30;

			bool[] grid = new bool[gw*gh];
			RandWalk(grid, gw / 2, gh / 2, gw, gh, 50);

			for (int w=0;w<0;w++)
			for (int y = 1; y < gh-1; y++)
			{
				for (int x = 1; x < gw-1; x++)
				{
					int i = y * gw + x;

						if (grid[i - 1] && grid[i + 1] && grid[i - gw] && grid[i + gw]
							&& grid[i - 1 - gw] && grid[i + 1 - gw] && grid[i - 1 + gw] && grid[i + 1 + gw])
						grid[i] = false;
				}
			}

			List<Point> pts = new List<Point>();
			List<Polygon> polys = new List<Polygon>();

			for (int y = 0; y < gh; y++)
			{
				for (int x = 0; x < gw; x++)
				{
					Point p = new Point();
					p.x = scalex * (x + (float)_rand.NextDouble());
					p.y = scaley * (y + (float)_rand.NextDouble());
					pts.Add(p);

					if (x < (gw - 1) && y < (gh - 1))
					{
						if (grid[y * gw + x])
						{
							Polygon np = new Polygon();
							np.indices = new int[3];
							np.indices[0] = y * gw + x;
							np.indices[1] = y * gw + x + 1;
							np.indices[2] = y * gw + x + 1 + gw;
							polys.Add(np);
							np = new Polygon();
							np.indices = new int[3];
							np.indices[0] = y * gw + x;
							np.indices[1] = y * gw + x + 1 + gw;
							np.indices[2] = y * gw + x + gw;
							polys.Add(np);
						}
					}
				}
			}
				
			List<Polygon>[] shared = new List<Polygon>[pts.Count * pts.Count];
			foreach (Polygon pk in polys)
			{
				for (int k = 0; k < pk.indices.Length; k++)
				{
					int pka = pk.indices[k];
					int pkb = pk.indices[(k + 1) % pk.indices.Length];
					int si = EdgeIndex(pka, pkb, pts.Count);
					if (shared[si] == null)
						shared[si] = new List<Polygon>();
					shared[si].Add(pk);
				}
			}

			while (true)
			{
				bool succ = false;

				List<Edge> order = new List<Edge>();
				for (int _ta = 0; _ta < pts.Count; _ta++)
				{
					for (int _tb = _ta + 1; _tb < pts.Count; _tb++)
					{
						if (shared[EdgeIndex(_ta, _tb, pts.Count)] == null)
							continue;

						Edge e = new Edge();
						e.a = _ta;
						e.b = _tb;

//						order.Add(e);
						order.Insert(_rand.Next()%(order.Count+1), e);
					}
				}

				Console.WriteLine("starting with " + order.Count + " possibilities");
				foreach (Edge e in order)
				{
					int ta = e.a;
					int tb = e.b;
					{
						int ei = EdgeIndex(ta, tb, pts.Count);
						List<Polygon> cand = shared[ei];
						if (cand == null || cand.Count < 2)
							continue;

						Console.WriteLine("examiing join " + ta + " => " + tb + " cand " + cand.Count);

						Polygon merge0 = cand[0];
						Polygon merge1 = cand[1];

						if (merge0 == merge1)
							Console.WriteLine("MERGING SAME!!");
						if (merge0.junked)
							Console.WriteLine("MERGE0 IS JUNKED");
						if (merge1.junked)
							Console.WriteLine("MERGE1 IS JUNKED");

						List<int> newIndices = new List<int>();
						List<int> rmEdges = new List<int>();
						List<int> common = new List<int>();

						foreach (int c0 in merge0.indices)
						{
							foreach (int c1 in merge1.indices)
							{
								if (c0 == c1)
								{
									common.Add(c0);
								}
							}
						}

						int insertion = 0;
						for (int ip0 = 0; ip0 < merge0.indices.Length; ip0++)
						{
							if (!common.Contains(merge0.indices[ip0]))
							{
								newIndices.Add(merge0.indices[ip0]);
							}
							else
							{
								insertion = newIndices.Count;
							}
						}

						int ip1 = 0;
						bool wasin = false;
						bool wasout = false;
						bool done = false;
						int le = -1;

						// 1. find first non-common that follows a common
						// 2. add all the non-common vertices
						// 3. add the common edges to rmEdges
						// 4. done.
						while (true)
						{
							if (common.Contains(merge1.indices[ip1]))
							{
								if (!wasout)
								{
									wasin = true;
								}
								else
								{
									if (!done)
									{
										// add the first shared
										newIndices.Insert(insertion++, merge1.indices[ip1]);
										done = true;
									}
									if (le != -1)
										rmEdges.Add(EdgeIndex(le, merge1.indices[ip1], pts.Count));
									le = merge1.indices[ip1];
								}
							}
							else if (wasin)
							{
								if (done)
								{
									break;
								}

								// out of shared
								if (!wasout)
								{
									// add the first shared
									newIndices.Insert(insertion++, merge1.indices[(ip1 - 1 + merge1.indices.Length)%merge1.indices.Length]);
									wasout = true;
								}
								newIndices.Insert(insertion++, merge1.indices[ip1]);
							}
							ip1 = (ip1 + 1) % merge1.indices.Length;
						}

						if (!IsConvex(pts, newIndices))
						{
							Console.WriteLine("Non-convex result on the " + newIndices.Count + "-gon");
							continue;
						}

						foreach (int t in newIndices)
							Console.Write(t + " ");

						Console.WriteLine(" are result edges");
						Console.WriteLine("result " + newIndices);
						Console.WriteLine("merged into a " + newIndices.Count + "-gon");
						merge0.indices = newIndices.ToArray();
					
						foreach (int rme in rmEdges)
						{
							while (shared[rme].Remove(merge1))
								;
							while (shared[rme].Remove(merge0))
								;
						}

						// update edge0
						for (int i=0;i<newIndices.Count;i++)
						{
							int ne = EdgeIndex(newIndices[i], newIndices[(i + 1) % newIndices.Count], pts.Count);
							while (shared[ne].Remove(merge0))
								;
							while (shared[ne].Remove(merge1))
								;
							shared[ne].Add(merge0);
						}
	
						merge1.junked = true;
						polys.Remove(merge1);
						succ = true;
					}
				}
				if (!succ)
					break;
			}


			NavGraph ng = new NavGraph();
			ng.m_polys = polys.ToArray();
			ng.m_pts = pts.ToArray();
			return ng;
		}
	}
}

