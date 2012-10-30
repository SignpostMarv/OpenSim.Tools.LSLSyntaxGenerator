<?php
/*
 * This file is dual-licensed. In addition to the license that the
 *	OpenSim.Tools.LSLSyntaxGenerator is made available under, it is also
 *	available under the GPL.
 * GPL License:
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301 USA
*/
function geshi_ossl($context){
    $context->useStandardIntegers();
    $context->useStandardDoubles();
    $context->setCharactersDisallowedBeforeKeywords(array('$A-Za-z_'));
    $context->addChild('double_string', 'string');
	
	$context->addKeywordGroup(array(
		'if', 'else',
		'for', 'while', 'do', 'jump',
		'state', 'default'
	), 'keyword', true);
	
	$context->addKeywordGroup(array(
		'integer',
		'float',
		'string',
		'key',
		'vector',
		'rotation',
		'list'
	), 'types', true);
	
	$context->addKeywordGroup(array(
		'at_rot_target', 'not_at_rot_target',
		'at_target', 'not_at_target',
		'changed',
		'collision', 'collision_start', 'collision_end',
		'dataserver',
		'email',
		'http_request', 'http_response',
		'land_collision', 'land_collision_start', 'land_collision_end',
		'link_message',
		'listen',
		'money',
		'moving_end', 'moving_start',
		'object_rez', 'on_rez',
		'sensor', 'no_sensor',
		'state_entry', 'state_exit',
		'remote_data',
		'run_time_permissions',
		'timer',
		'touch', 'touch_start', 'touch_end'
	), 'events', true);

	if(!file_exists(__DIR__ . DIRECTORY_SEPARATOR . 'ossl.json')){
		return;
	}

	$_osslJSON = json_decode(file_get_contents(__DIR__ . DIRECTORY_SEPARATOR . 'ossl.json'), true);
	
	if(!$_osslJSON){
		return;
	}


	$context->addKeywordGroup(
			$_osslJSON['ScriptConstants'], 'ScriptConstants', true);

	foreach($_osslJSON as $api => $v){
		if($api === 'ScriptConstants'){
			continue;
		}
		$context->addKeywordGroup(array_keys($_osslJSON[$api]), $api, true);
	}
}


function geshi_ossl_double_string (&$context)
{
    $context->addDelimiters('"', '"');
    $context->addEscapeGroup(array('\\'), array(
        'n', 'r', 't',
        '\\', '"'));
}
?>