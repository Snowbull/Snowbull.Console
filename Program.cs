/**
 * Entry Point for Snowbull.
 *
 * Copyright 2016 by Lewis Hazell <staticabc@live.co.uk>
 *
 * This file is part of Snowbull.
 * 
 * Snowbull is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version
 * 3 of the License, or (at your option) any later version.
 * 
 * Snowbull is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty 
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See
 * the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Snowbull. If not, see <http://www.gnu.org/licenses/>.
 *
 * License: GPL-3.0 <https://www.gnu.org/licenses/gpl-3.0.txt>
 */

using System;
using System.Net;
using System.Reflection;
using System.Collections.Generic;
using Akka.Actor;
using Core = Snowbull.Core;
using Configuration = Snowbull.Core.Configuration;

namespace Snowbull.Console {
    class Program {
        public static void Main(string[] args) {
            Configuration.SnowbullConfigurationSection config = Configuration.SnowbullConfigurationSection.GetConfiguration();
            Core.Snowbull[] instances = new Core.Snowbull[config.Servers.Count];
			for(int i = 0; i < config.Servers.Count; i++) {
                Core.Configuration.Server s = config.Servers[i];
				Dictionary<string, Core.ZoneInitialiser> zones = new Dictionary<string, Core.ZoneInitialiser>();
                foreach(Core.Configuration.Zone zone in s.Zones) {
					Type za = Type.GetType(zone.Type);
					ConstructorInfo constructor = za.GetConstructor(new Type[] { typeof(string), typeof(IActorContext), typeof(Core.Server) });
					zones.Add(zone.Name, (context, server) => (Core.Zone) constructor.Invoke(new object[] { zone.Name, context, server }));
				}
                Core.Snowbull instance = new Core.Snowbull(s.Name, zones);
				instance.Server.Bind(new IPEndPoint(IPAddress.IPv6Any, int.Parse(s.Port)));
				instances[i] = instance;
			}
			System.Console.ReadLine();
        }
    }
}
