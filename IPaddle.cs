using System;
using AllegroSharp;
namespace Pong
{
    public interface IPaddle
    {
        Point Position
        {
            get;
        }
        Color Color
        {
            get;
        }
    }
}

