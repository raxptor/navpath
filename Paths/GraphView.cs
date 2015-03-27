using System;
using System.Collections.Generic;

namespace Paths
{
	[System.ComponentModel.ToolboxItem (true)]
	public class PathView : Gtk.DrawingArea
	{
		NavGraph m_ng;

		public PathView()
		{
			m_ng = NavGraph.MakeRandom(new Random());
		}

		public void SetView(NavGraph ng)
		{
			m_ng = ng;
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			// Insert button press handling code here.
			return base.OnButtonPressEvent (ev);
		}

		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			base.OnExposeEvent (ev);

			// Insert drawing code here.
			Gdk.GC g = new Gdk.GC(GdkWindow);
			g.RgbFgColor = new Gdk.Color(0,0,100);
			g.RgbBgColor = new Gdk.Color(0,0,0);

			for (int i = 0; i < m_ng.m_polys.Length; i++)
			{
				NavGraph.Polygon p = m_ng.m_polys[i];
				List<Gdk.Point> pts = new List<Gdk.Point>();
				for (int j = 0; j < p.indices.Length; j++)
				{
					NavGraph.Point k = m_ng.m_pts[p.indices[j]];
					pts.Add(new Gdk.Point((int)k.x, (int)k.y));
				}

				g.RgbFgColor = new Gdk.Color(200,200,255);
				GdkWindow.DrawPolygon(g, true, pts.ToArray());
				g.RgbFgColor = new Gdk.Color(0,0,0);
				GdkWindow.DrawPolygon(g, false, pts.ToArray());
//				GdkWindow.DrawLine(g, (int)p0.x, (int)p0.y, (int)p1.x, (int)p1.y);
			}

/*
 * 
 * 

			for (int i=0;i<m_widget.get_layers_size();i++)
			{
				inki.UIElementLayer layer = m_widget.get_layers(i);
				if (layer != null)
				{
					for (int j=0;j<layer.get_elements_size();j++)
					{
						inki.UIElement el = layer.resolve_elements(j);
						if (el != null)
						{
							inki.UIRect lr = el.get_layout();						
							Gdk.Rectangle r = new Gdk.Rectangle((int)lr.get_x(), (int)lr.get_y(), (int)lr.get_width(), (int)lr.get_height());
							Console.WriteLine("And it is " + el.m_mi.GetPutkiType().Name + " " + r);
							GdkWindow.DrawRectangle(g, false, r);
						}
					}

				}
			}

		*/
			return true;
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			// Insert layout code here.
		}

		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			// Calculate desired size here.
			requisition.Height = 700;
			requisition.Width = 1280;
		}
	}
}

