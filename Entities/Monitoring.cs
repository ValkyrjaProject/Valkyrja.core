using System;
using Prometheus;

namespace Valkyrja.entities
{
	public class Monitoring: IDisposable
	{
		private readonly MetricPusher Prometheus;

		public readonly Counter Disconnects = Metrics.CreateCounter("discord_valk_dc", "Valkyrja: disconnects");
		public readonly Counter Error500s = Metrics.CreateCounter("discord_valk_500", "Valkyrja: Discord server error 500s");
		public readonly Counter Messages = Metrics.CreateCounter("discord_valk_msg", "Valkyrja: Messages received");
		public readonly Counter Commands = Metrics.CreateCounter("discord_valk_cmd", "Valkyrja: Commands executed");
		public readonly Counter AntispamDeletes = Metrics.CreateCounter("discord_valk_cmd", "Valkyrja: Commands executed");
		public readonly Counter Mutes = Metrics.CreateCounter("discord_valk_cmd", "Valkyrja: Commands executed");
		public readonly Counter Bans = Metrics.CreateCounter("discord_valk_cmd", "Valkyrja: Commands executed");

		public Monitoring(DbConfig config, int shardId)
		{
			if( this.Prometheus == null )
				this.Prometheus = new MetricPusher(config.PrometheusEndpoint, config.PrometheusJob, shardId.ToString(), config.PrometheusInterval);
			this.Prometheus.Start();
		}

		public void Dispose()
		{
			this.Prometheus.Stop();
			((IDisposable)this.Prometheus)?.Dispose();
		}
	}
}
