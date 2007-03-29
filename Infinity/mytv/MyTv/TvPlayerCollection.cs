using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using TvControl;

namespace MyTv
{
  class TvPlayerCollection
  {
    #region variables
    static TvPlayerCollection _instance = null;
    List<TvMediaPlayer> _players = new List<TvMediaPlayer>();
    #endregion

    #region static ctor
    /// <summary>
    /// Gets the TvPlayerCollection instance.
    /// </summary>
    /// <value>The TvPlayerCollection instance.</value>
    public static TvPlayerCollection Instance
    {
      get
      {
        if (_instance == null)
          _instance = new TvPlayerCollection();
        return _instance;
      }
    }
    #endregion

    #region public methods
    /// <summary>
    /// creates and returns a new media player
    /// </summary>
    /// <param name="card">The card.</param>
    /// <param name="uri">The URI.</param>
    /// <returns></returns>
    public TvMediaPlayer Get(VirtualCard card, string fileName)
    {
      if (!File.Exists(fileName))
      {
        TvServer server = new TvServer();
        if (fileName == card.TimeShiftFileName)
          fileName = card.RTSPUrl;
        else
          fileName = server.GetRtspUrlForFile(fileName);
      }
      string fname = fileName;
      if (fileName.StartsWith("rtsp://"))
      {
        fname = String.Format(@"{0}\1.tsp", Directory.GetCurrentDirectory());
        if (File.Exists(fname))
        {
          File.Delete(fname);
        }
        using (FileStream stream = new FileStream(fname, FileMode.OpenOrCreate))
        {
          using (BinaryWriter writer = new BinaryWriter(stream))
          {
            byte k=0x12;
            for (int i = 0; i < 99; ++i) writer.Write(k);
            writer.Write(fileName);
          }
        }
      }
      Uri uri = new Uri(fname, UriKind.Absolute);
      TvMediaPlayer player = new TvMediaPlayer(card);
      player.Open(uri);
      _players.Add(player);
      return player;
    }


    /// <summary>
    /// Releases the specified player.
    /// </summary>
    /// <param name="player">The player.</param>
    public void Release(TvMediaPlayer player)
    {
      for (int i = 0; i < _players.Count; ++i)
      {
        if (player == _players[i])
        {
          _players.RemoveAt(i);
          return;
        }
      }
    }

    /// <summary>
    /// Gets the <see cref="MyTv.TvMediaPlayer"/> at the specified index.
    /// </summary>
    /// <value></value>
    public TvMediaPlayer this[int index]
    {
      get
      {
        return _players[index];
      }
    }

    /// <summary>
    /// Gets the number of players active.
    /// </summary>
    /// <value>The number of active players.</value>
    public int Count
    {
      get
      {
        return _players.Count;
      }
    }
    /// <summary>
    /// Disposes all players
    /// </summary>
    public void DisposeAll()
    {
      List<TvMediaPlayer> players = new List<TvMediaPlayer>();
      foreach (TvMediaPlayer player in _players)
      {
        players.Add(player);
      }

      foreach (TvMediaPlayer player in players)
      {
        player.Dispose(true);
      }
      _players.Clear();
    }
    #endregion
  }
}
