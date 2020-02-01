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
		public readonly Counter AntispamActions = Metrics.CreateCounter("discord_valk_antispam", "Valkyrja: Antispam actions");
		public readonly Counter MsgsDeleted = Metrics.CreateCounter("discord_valk_del", "Valkyrja: Messages deleted");
		public readonly Counter Mutes = Metrics.CreateCounter("discord_valk_mute", "Valkyrja: Mutes issued");
		public readonly Counter Bans = Metrics.CreateCounter("discord_valk_ban", "Valkyrja: Bans issued");

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
