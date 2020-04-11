// Module_SpriteFont - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_SpriteFont;
using static LastnFurious.SpriteFontStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_SpriteFont
    {
        // Fields

        // Methods
    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {

    }

    #endregion

    #region SpriteFont (AGS struct from .ash converted to class)

    public class SpriteFont
    {
        // Fields
        public DynamicSprite[] Glyphs;
        public int FirstGlyph;
        public int LastGlyph;
        public int Height;
        public int GlyphWidth;
        public int[] Offs;
        public int[] Widths;

        // Methods
        public void Delete()
        {
            int count = this.LastGlyph - this.FirstGlyph + 1;
            int i = 0;
            if (this.Glyphs != null)
            {
                for (i = 0; i < count; i += 1)
                {
                    this.Glyphs[i].Delete();
                }
            }
            this.Glyphs = null;
            this.Offs = null;
            this.Widths = null;
        }

        public void CreateFromSprite(DynamicSprite sprite, int gl_width, int height, int gl_first, int gl_last, int[] offs, int[] widths)
        {
            this.Delete();
            int sprw = sprite.Width;
            int sprh = sprite.Height;
            int in_row = sprw / gl_width;
            int total = in_row * (sprh / height);
            total = Maths.Min(total, gl_last - gl_first + 1);
            if (total <= 0)
                return;
            this.Glyphs = new DynamicSprite[total];
            this.FirstGlyph = gl_first;
            this.LastGlyph = gl_last;
            this.GlyphWidth = gl_width;
            this.Height = height;
            int i = 0;
            if (offs != null)
            {
                this.Offs = offs;
            }
            else 
            {
                this.Offs = new int[total];
                for (i = 0; i < total; i += 1)
                {
                    this.Offs[i] = 0;
                }
            }
            if (widths != null)
            {
                this.Widths = widths;
            }
            else 
            {
                this.Widths = new int[total];
                for (i = 0; i < total; i += 1)
                {
                    this.Widths[i] = gl_width;
                }
            }
            int x = 0;
            int  y = 0;
            int gl = gl_first;
            DrawingSurface ds = sprite.GetDrawingSurface();
            for (y = 0; y < sprh   && gl <= gl_last; y = y + height)
            {
                for (x = 0; x < sprw   && gl <= gl_last; x = x + gl_width)
                {
                    DynamicSprite spr = DynamicSprite.CreateFromDrawingSurface(ds, x + this.Offs[gl], y, this.Widths[gl], height);
                    this.Glyphs[gl] = spr;
                    gl += 1;
                }
            }
            ds.Release();
        }

        public void DrawText(String s, DrawingSurface ds, int x, int y)
        {
            if (this.Glyphs == null)
                return;
            int i = 0;
            for (i = 0; i < s.Length; i += 1)
            {
                int gl = s.Chars()[i];
                if (gl >= this.FirstGlyph && gl <= this.LastGlyph)
                {
                    ds.DrawImage(x, y, this.Glyphs[gl].Graphic);
                    x += this.Widths[gl];
                }
            }
        }

        public int GetTextWidth(String s)
        {
            if (this.Glyphs == null)
                return 0;
            int width = 0;
            int i = 0;
            for (i = 0; i < s.Length; i += 1)
            {
                int gl = s.Chars()[i];
                if (gl >= this.FirstGlyph && gl <= this.LastGlyph)
                {
                    width += this.Widths[gl];
                }
            }
            return width;
        }

        public void DrawTextCentered(String s, DrawingSurface ds, int x, int y, int width)
        {
            int textw = this.GetTextWidth(s);
            this.DrawText(s, ds, x + (width - textw) / 2, y);
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class SpriteFontStaticRef
    {
    }

    #endregion
    
}
