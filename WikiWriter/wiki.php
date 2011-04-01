<?php
include_once 'geshi/geshi.php';
$language = $argv[1];
$source = file_get_contents($argv[2]);
$geshi = new GeSHi($source, $language);
file_put_contents($argv[3], $geshi->parse_code());
?>
